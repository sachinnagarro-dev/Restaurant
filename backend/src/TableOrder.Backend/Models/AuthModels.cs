using System.ComponentModel.DataAnnotations;

namespace TableOrder.Backend.Models;

public class LoginRequest
{
    [Required]
    public string Username { get; set; } = string.Empty;
    
    [Required]
    public string Password { get; set; } = string.Empty;
}

public class LoginResponse
{
    public string Token { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
}

public class UserRole
{
    public const string Admin = "Admin";
    public const string Staff = "Staff";
}
