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
    private readonly IStatsService _statsService;


    public ProfileController(IStatsService statsService)
    {
        _statsService = statsService;
    }
    private string GetUserId()
    {
        var id = User.FindFirst(ClaimTypes.NameIdentifier);
        if (id == null || string.IsNullOrEmpty(id.Value)) throw new UnauthorizedAccessException();
        return id.Value;
    }
    [HttpGet("profile")]
    public async Task<IActionResult> GetProfile()
    {
        var id = GetUserId();
        var stats = await _statsService.GetUserStats(id);
        Response.Headers["Cache-Control"] = "no-cache, no-store, must-revalidate";
        Response.Headers["Pragma"] = "no-cache";
        Response.Headers["Expires"] = "0";
        return Ok(stats);
    }
}