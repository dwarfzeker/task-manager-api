using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using todoApp.Core.Interfaces;
using todoApp.Core.Entities;
using todoApp.Core.DTOs;
using todoApp.Core.Events;

namespace todoApp.Web.Controllers;

[Authorize]
[ApiController] 
[Route("api/tasks")]
[EnableRateLimiting("api")]
public class TaskController : ControllerBase
{
    private readonly ITodoRepository _todoRepository;
    private readonly ILogger<TaskController> _logger;
    private readonly IEventHandler<TaskCompletedEvent> _completedHandler;
    private readonly IEventHandler<TaskDeletedEvent> _deletedEvent;
    private readonly ICachedStatsService _cachedStatsService;
    private readonly IUserRepository _userRepository;
    public TaskController(ITodoRepository todoRepository, 
        ILogger<TaskController> logger,
        IEventHandler<TaskCompletedEvent> completedEvent,
        IEventHandler<TaskDeletedEvent> deletedEvent,
       ICachedStatsService cachedStatsService,
        IUserRepository userRepository)
    {
        _todoRepository = todoRepository;
        _logger = logger;
        _completedHandler = completedEvent;
        _cachedStatsService = cachedStatsService;
        _userRepository = userRepository;
        _deletedEvent = deletedEvent;
    }

    public string GetUserId()
    { 
        var id =  User.FindFirst(ClaimTypes.NameIdentifier)?.Value
               ?? throw new UnauthorizedAccessException($"user not authenticated. please provide a valid token. token you gave is {ClaimTypes.NameIdentifier}");
        _logger.LogDebug($"id in GetUserId is {id}");
        _logger.LogDebug($"claim in GetUserId is {ClaimTypes.NameIdentifier}");
        return id;
    }

    [HttpGet]
 
    public async Task<IActionResult> GetAll([FromQuery] DateTime? date)
    {
        var id = GetUserId();
        _logger.LogDebug($"token from query is {Request.Headers["Authorization"].FirstOrDefault()}");
        if (date == null)
        {
            var allTasks = await _todoRepository.GetByUserAsync(id);

            var response = TakeListResponseDTO.FromTasks(allTasks);
            return Ok(response);
        }

        var tasks = await _todoRepository.GetByUserAndDateAsync(id, date.Value);

        var dto = TakeListResponseDTO.FromTasks(tasks);
        return Ok(dto);

    }
    
    [HttpGet("{id}")]
    public async Task<IActionResult> Get(Guid id)
    {
        var userid = GetUserId();
        var task = await _todoRepository.GetByIdAsync(id);
        if  (task == null) return NotFound();
        if (userid != task.UserId) return Forbid();
        var dto = TakeResponseDTO.FromEntity(task);
        return Ok(dto);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateTaskDTO? dto)
    {
        var userId = GetUserId();
        if (dto == null) return BadRequest("empty new task");
        if (string.IsNullOrWhiteSpace(dto.Title)) return BadRequest("empty task title");
        if (dto.Title.Length > 100) return BadRequest("task title too long");
        var entity = new ListItem
        {
            Title = dto.Title,
            UserId = userId,
            Description = dto.Description,
            IsCompleted = false,
            CreatedAt = DateTime.UtcNow,
            TargetDate = dto.TargetDate?.Date ?? DateTime.UtcNow.Date,
            PositionX = Random.Shared.NextDouble() * 70 + 10,
            PositionY = Random.Shared.NextDouble() * 70 + 10
        };
        Console.WriteLine($"Created entity with Title: '{entity.Title}'");
        await _todoRepository.AddAsync(entity);
        Console.WriteLine($"Saved entity with Id: {entity.Id}");
        return CreatedAtAction(nameof(Get), new { id = entity.Id }, entity);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var task = await _todoRepository.GetByIdAsync(id);
        var userid = GetUserId();
        if (task == null) return NotFound();
        if (userid != task.UserId) return NotFound();
        await _todoRepository.DeleteAsync(id);
        var deleted = new TaskDeletedEvent(task.Id.ToString(), userid, DateTime.UtcNow);
    
        return NoContent();
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Change(Guid id, [FromBody] UpdateTaskDTO? dto)
    {
        var userd = GetUserId();
        if (dto == null) return BadRequest("empty new task");
        var oldTask = await _todoRepository.GetByIdAsync(id);
        if (oldTask == null) return NotFound();
        if (userd != oldTask.UserId) return NotFound();
        oldTask.Title = dto.Title;
        oldTask.IsCompleted = dto.IsCompleted;
        oldTask.Description =  dto.Description;
        
        await _todoRepository.UpdateAsync(oldTask);
        return Ok(oldTask);
    }

    [HttpGet("recent")]
    public async Task<IActionResult> RecentTasks()
    {
        var user = GetUserId();
        var lastViewKey = $"last_view_{user}";
        var lastView = HttpContext.Session.GetString(lastViewKey);
        HttpContext.Session.SetString(lastViewKey, DateTime.UtcNow.ToString());
        var tasks = await _todoRepository.GetByUserAsync(user);
        return Ok(new
        {
            tasks,
            lastVisit = lastView ?? "First visit!",
            sessionId = HttpContext.Session.Id
        });
    }

    [HttpPost("draft")]
    public IActionResult SaveDraft([FromBody] DraftDTO draft)
    {
        var user = GetUserId();
        var draftKey = $"draft_{user}";
        HttpContext.Session.SetString(draftKey, JsonSerializer.Serialize(draft));
        return Ok(new {saved = true});
    }

    [HttpGet("draft")]
    public IActionResult GetDraft()
    {
        var user = GetUserId();
        var draftKey = $"draft_{user}";
        var draft = HttpContext.Session.GetString(draftKey);
        if (draft == null) return NotFound();
        return Ok(JsonSerializer.Deserialize<DraftDTO>(draft));
    }
        
    [HttpPatch("id/{toggle}")]
    public async Task<IActionResult> ToggleCompleted(Guid id, [FromBody] PatchTasksDTO dto)
    {
        var user = GetUserId();
        var task = await _todoRepository.GetByIdAsync(id);
        if (task == null || user != task.UserId) return NotFound();
        var wasCompleted = task.IsCompleted;
        var isCompleted = dto.IsCompleted ?? false;
        if (dto.Title != null) task.Title = dto.Title;
        if (dto.Description != null) task.Description = dto.Description;
        if (dto.IsCompleted != null) task.IsCompleted = dto.IsCompleted.Value;
        task.IsCompleted = isCompleted;
        await _todoRepository.UpdateAsync(task);

        if (!wasCompleted && isCompleted)
        {
            var @event = new TaskCompletedEvent
            (
                task.Id.ToString(),
                user,
                DateTime.UtcNow
            );
            await _completedHandler.HandleAsync(@event);
            
            return Ok(task);

        }

        await _todoRepository.UpdateAsync(task);
        
        return Ok(task);
    }

    [HttpPatch("{id}/position")]
    public async Task<IActionResult> UpdatePosition(Guid id, [FromBody] UpdatePositionDTO dto)
    {
        var user = GetUserId();
        var task = await _todoRepository.GetByIdAsync(id);
        if (task == null || user != task.UserId) return NotFound();
        task.PositionX = dto.PositionX;
        task.PositionY = dto.PositionY;
        await _todoRepository.UpdateAsync(task);
        return Ok(new UpdatePositionDTO 
        {
            Id = task.Id,
            PositionX = task.PositionX.Value,
            PositionY = task.PositionY.Value
        });

    }
}