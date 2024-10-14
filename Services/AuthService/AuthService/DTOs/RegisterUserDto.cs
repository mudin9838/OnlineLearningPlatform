using System.ComponentModel.DataAnnotations;

namespace AuthService.DTOs;

public class RegisterUserDto
{
    [Required]
    [EmailAddress]
    public string Email { get; set; }

    [Required]
    [MinLength(6)]
    public string Password { get; set; }

    [Required]
    [Compare("Password", ErrorMessage = "Passwords do not match")]
    public string ConfirmPassword { get; set; }

    public string FirstName { get; set; }
    public string LastName { get; set; }
}
