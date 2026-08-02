using System.ComponentModel.DataAnnotations;

namespace FleetApi.DTOs;

public class UserAuthDto
{
    [Required]
    public string Username { get; set; } = string.Empty;

    [Required]
    [MinLength(6, ErrorMessage = "Пароль должен быть не менее 6 символов")]
    public string Password { get; set; } = string.Empty;
}