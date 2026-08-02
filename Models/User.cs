namespace FleetApi.Models;

public class User
{
    public int Id { get; set;}
    public string Username { get; set;} = string.Empty;
    // Храним только кэш, а не сам пароль
    public string PasswordHash { get; set;} = string.Empty;
}