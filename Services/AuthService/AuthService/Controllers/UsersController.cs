using AuthService.DTOs;
using AuthService.Models;
using AuthService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NotificationService.Models;
using NotificationService.Services;

namespace AuthService.Controllers;
[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly ITokenService _tokenService;
    private readonly NotificationPublisher _notificationPublisher;
    public UserController(UserManager<ApplicationUser> userManager,
                          SignInManager<ApplicationUser> signInManager,
                          ITokenService tokenService, RoleManager<IdentityRole> roleManager, NotificationPublisher notificationPublisher)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _tokenService = tokenService;
        _roleManager = roleManager;
        _notificationPublisher = notificationPublisher;
    }

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register(RegisterUserDto registerDto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var user = new ApplicationUser
        {
            UserName = registerDto.Email,
            Email = registerDto.Email,
            FirstName = registerDto.FirstName,
            LastName = registerDto.LastName
        };

        var result = await _userManager.CreateAsync(user, registerDto.Password);
        if (!result.Succeeded) return BadRequest(result.Errors);

        // Publish a notification after successful registration
        var notification = new Notification
        {
            Id = Guid.NewGuid().ToString(),
            Message = $"User '{user.Id}' registered successfully.",
            CreatedAt = DateTime.UtcNow
        };
        _notificationPublisher.Publish(notification);

        return Ok("Registration successful!");
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login(LoginUserDto loginDto)
    {
        var user = await _userManager.FindByEmailAsync(loginDto.Email);
        if (user == null) return Unauthorized("Invalid login attempt");

        var result = await _signInManager.PasswordSignInAsync(user, loginDto.Password, false, false);
        if (!result.Succeeded) return Unauthorized("Invalid login attempt");

        var token = _tokenService.GenerateToken(user);
        return Ok(new { Token = token });
    }

    [Authorize(Roles = "Admin")]
    [HttpPost("assign-role")]
    public async Task<IActionResult> AssignRole(string userId, string role)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return NotFound("User not found");

        var roleExists = await _roleManager.RoleExistsAsync(role);
        if (!roleExists) return BadRequest("Role does not exist");

        var result = await _userManager.AddToRoleAsync(user, role);
        return !result.Succeeded ? BadRequest(result.Errors) : Ok($"Role '{role}' assigned to user '{user.UserName}'");
    }
}
