using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using AnprParking.Api.Domain.Entities;

namespace AnprParking.Api.Controllers;

public record MeResponse(
    string Id,
    string Username,
    string? Email,
    DateTime CreatedUtc,
    string Role
);

[Authorize]
[ApiController]
[Route("api/users")]
public class UsersController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _users;

    public UsersController(UserManager<ApplicationUser> users)
    {
        _users = users;
    }

    // GET /api/users/me
    [HttpGet("me")]
    public async Task<ActionResult<MeResponse>> Me()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized();

        var u = await _users.FindByIdAsync(userId);
        if (u == null) return Unauthorized();

        var roles = await _users.GetRolesAsync(u);
        var role = roles.Contains("Admin") ? "Admin" : (roles.FirstOrDefault() ?? "User");

        return Ok(new MeResponse(
            Id: u.Id,
            Username: u.UserName ?? "",
            Email: u.Email,
            CreatedUtc: u.CreatedUtc,
            Role: role
        ));
    }
}
