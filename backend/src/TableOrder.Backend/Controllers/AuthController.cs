using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using TableOrder.Backend.Models;
using TableOrder.Backend.Services;

namespace TableOrder.Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IJwtService _jwtService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        IJwtService jwtService,
        IConfiguration configuration,
        ILogger<AuthController> logger)
    {
        _jwtService = jwtService;
        _configuration = configuration;
        _logger = logger;
    }

    /// <summary>
    /// Login with username and password
    /// </summary>
    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request)
    {
        try
        {
            // For prototype, check against configured credentials
            var adminUsername = _configuration["Auth:Admin:Username"] ?? "admin";
            var adminPassword = _configuration["Auth:Admin:Password"] ?? "admin123";
            var staffUsername = _configuration["Auth:Staff:Username"] ?? "staff";
            var staffPassword = _configuration["Auth:Staff:Password"] ?? "staff123";

            string role;
            if (request.Username == adminUsername && request.Password == adminPassword)
            {
                role = UserRole.Admin;
            }
            else if (request.Username == staffUsername && request.Password == staffPassword)
            {
                role = UserRole.Staff;
            }
            else
            {
                _logger.LogWarning($"Failed login attempt for username: {request.Username}");
                return Unauthorized(new { message = "Invalid username or password" });
            }

            var token = _jwtService.GenerateToken(request.Username, role);
            var expiresAt = DateTime.UtcNow.AddMinutes(int.Parse(_configuration["Jwt:ExpirationMinutes"] ?? "60"));

            _logger.LogInformation($"Successful login for user: {request.Username} with role: {role}");

            return Ok(new LoginResponse
            {
                Token = token,
                Role = role,
                ExpiresAt = expiresAt
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login");
            return StatusCode(500, new { message = "An error occurred during login" });
        }
    }

    /// <summary>
    /// Validate admin key (for backward compatibility)
    /// </summary>
    [HttpPost("validate")]
    public async Task<ActionResult> ValidateAdminKey([FromBody] AdminKeyRequest request)
    {
        try
        {
            var adminKey = _configuration["Auth:AdminKey"] ?? "admin123";
            
            if (request.AdminKey == adminKey)
            {
                // Generate token for admin role
                var token = _jwtService.GenerateToken("admin", UserRole.Admin);
                var expiresAt = DateTime.UtcNow.AddMinutes(int.Parse(_configuration["Jwt:ExpirationMinutes"] ?? "60"));

                return Ok(new LoginResponse
                {
                    Token = token,
                    Role = UserRole.Admin,
                    ExpiresAt = expiresAt
                });
            }

            _logger.LogWarning($"Invalid admin key attempt");
            return Unauthorized(new { message = "Invalid admin key" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating admin key");
            return StatusCode(500, new { message = "An error occurred during validation" });
        }
    }

    /// <summary>
    /// Get current user info from JWT token
    /// </summary>
    [HttpGet("me")]
    [Authorize]
    public ActionResult<UserInfo> GetCurrentUser()
    {
        var username = User.Identity?.Name ?? "Unknown";
        var role = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value ?? "Unknown";

        return Ok(new UserInfo
        {
            Username = username,
            Role = role
        });
    }
}

public class AdminKeyRequest
{
    public string AdminKey { get; set; } = string.Empty;
}

public class UserInfo
{
    public string Username { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
}
