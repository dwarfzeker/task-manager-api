using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using todoApp.Core.DTOs;
using todoApp.Core.Interfaces;

namespace todoApp.Web.Controllers;

[Authorize]
[ApiController]
[Route("api/auth")]
public class ProfileController : ControllerBase
{
    private readonly ICachedStatsService _statsService;


    public ProfileController(ICachedStatsService statsService)
    {
        _statsService = statsService;
    }
    private string GetUserId()
    {
        var id = User.FindFirst(ClaimTypes.NameIdentifier)!.Value ?? throw new UnauthorizedAccessException($"user not authenticated. please provide a correct token");
        return id;
    }
    [HttpGet("profile")]
    [ResponseCache(Duration = 60, Location = ResponseCacheLocation.Client)]
    public  IActionResult GetProfile()
    {
        var id = GetUserId();
        var stats = _statsService.GetUserStats(id).Result;

        return Ok(stats);
    }
}