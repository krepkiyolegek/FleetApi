namespace FleetApi.Models;

public class Car
{
    public int Id { get; set;}
    public string Make { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public int Year { get; set; }
    public string RegistrationNumber { get; set; } = string.Empty;
    public DateTime LastInspectionDate { get; set; }
}