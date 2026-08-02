using System.ComponentModel.DataAnnotations;


namespace FleetApi.DTOs;


public class CarCreateUpdateDto
{
    [Required]
    [StringLength(50)]
    public string Make { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string Model { get; set; } = string.Empty;

    [Range(1900, 2100)]
    public int Year { get; set; }

    [Required]
    [RegularExpression(@"^[A-Z0-9\-]{3,10}$", ErrorMessage = "Некорректный госномер")]
    public string RegistrationNumber { get; set; } = string.Empty;

    [Required]
    public DateTime LastInspectionDate { get; set; }
}