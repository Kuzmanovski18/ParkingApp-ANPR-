using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using AnprParking.Api.Domain.Entities;
using AnprParking.Api.Services.Auth;

namespace AnprParking.Api.Controllers;

//  Register: сите 3 задолжителни
public record RegisterRequest(string Username, string Email, string Password);

//  Login: email + password
public record LoginRequest(string Email, string Password);

public record AuthResponse(string Token, string Username, string Role);

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _users;
    private readonly SignInManager<ApplicationUser> _signIn;
    private readonly JwtTokenService _jwt;

    public AuthController(
        UserManager<ApplicationUser> users,
        SignInManager<ApplicationUser> signIn,
        JwtTokenService jwt)
    {
        _users = users;
        _signIn = signIn;
        _jwt = jwt;
    }

    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register([FromBody] RegisterRequest req)
    {
        var username = (req.Username ?? "").Trim();
        var email = (req.Email ?? "").Trim();

        if (username.Length < 3)
            return BadRequest("Username must be at least 3 characters.");

        if (string.IsNullOrWhiteSpace(email))
            return BadRequest("Email is required.");

        if (!email.Contains("@") || !email.Contains("."))
            return BadRequest("Email is not valid.");

        if (string.IsNullOrWhiteSpace(req.Password) || req.Password.Length < 6)
            return BadRequest("Password must be at least 6 characters.");

        //  duplicate username
        var existingByName = await _users.FindByNameAsync(username);
        if (existingByName != null)
            return Conflict("Username already exists.");

        //  duplicate email (required)
        var existingByEmail = await _users.FindByEmailAsync(email);
        if (existingByEmail != null)
            return Conflict("Email already exists.");

        var user = new ApplicationUser
        {
            UserName = username,
            Email = email
        };

        var create = await _users.CreateAsync(user, req.Password);
        if (!create.Succeeded)
            return BadRequest(create.Errors.Select(e => e.Description));

        // default role
        var addRole = await _users.AddToRoleAsync(user, "User");
        if (!addRole.Succeeded)
            return StatusCode(500, addRole.Errors.Select(e => e.Description));

        var roles = await _users.GetRolesAsync(user);
        var token = _jwt.CreateToken(user, roles);

        var role = roles.Contains("Admin") ? "Admin" : (roles.FirstOrDefault() ?? "User");
        return Ok(new AuthResponse(token, user.UserName!, role));
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest req)
    {
        var email = (req.Email ?? "").Trim();

        if (string.IsNullOrWhiteSpace(email))
            return BadRequest("Email is required.");

        if (string.IsNullOrWhiteSpace(req.Password))
            return BadRequest("Password is required.");

        //  login by email (instead of username)
        var user = await _users.FindByEmailAsync(email);
        if (user is null)
            return Unauthorized("Invalid credentials.");

        var ok = await _signIn.CheckPasswordSignInAsync(user, req.Password, lockoutOnFailure: false);
        if (!ok.Succeeded)
            return Unauthorized("Invalid credentials.");

        var roles = await _users.GetRolesAsync(user);
        var token = _jwt.CreateToken(user, roles);

        var role = roles.Contains("Admin") ? "Admin" : (roles.FirstOrDefault() ?? "User");
        return Ok(new AuthResponse(token, user.UserName!, role));
    }
}
