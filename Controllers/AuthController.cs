using FleetApi.Data;
using FleetApi.DTOs;
using FleetApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace FleetApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IConfiguration _configuration; // Для чтения секретного ключа JWT

    public AuthController(AppDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    // --- РЕГИСТРАЦИЯ ПОЛЬЗОВАТЕЛЯ ---
    [HttpPost("register")]
    public async Task<ActionResult> Register(UserAuthDto request)
    {
        // Проверяем, свободен ли логин
        if (await _context.Users.AnyAsync(u => u.Username == request.Username))
        {
            return BadRequest("Пользователь с таким именем уже существует.");
        }

        // БЕЗОПАСНОСТЬ: Хэшируем пароль перед сохранением в БД
        string passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

        var user = new User
        {
            Username = request.Username,
            PasswordHash = passwordHash
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return Ok("Пользователь успешно зарегистрирован.");
    }

    // --- ЛОГИН И ВЫДАЧА ТОКЕНА ---
    [HttpPost("login")]
    public async Task<ActionResult<string>> Login(UserAuthDto request)
    {
        // 1. Ищем пользователя в базе
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == request.Username);
        if (user == null)
        {
            return BadRequest("Неверное имя пользователя или пароль.");
        }

        // 2. БЕЗОПАСНОСТЬ: Проверяем, совпадает ли введенный пароль с хэшем из БД
        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            return BadRequest("Неверное имя пользователя или пароль.");
        }

        // 3. Если пароль верный, генерируем JWT токен
        string token = CreateToken(user);

        return Ok(new { token = token }); // Возвращаем JSON с токеном
    }

    // Вспомогательный метод для создания JWT
    private string CreateToken(User user)
    {
        // Вшиваем в токен информацию о пользователе (Claims)
        List<Claim> claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, user.Username)
        };

        // Достаем наш секретный ключ из конфигурации
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
        
        // Выбираем алгоритм шифрования
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        // Создаем сам токен
        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            claims: claims,
            expires: DateTime.UtcNow.AddDays(1), // Токен "протухнет" через 1 день (UTC!)
            signingCredentials: creds // Сервер берет наш Секретный Ключ и математически хеширует весь токен.
        );

        // Сериализуем его в строку
        var jwt = new JwtSecurityTokenHandler().WriteToken(token);
        return jwt;
    }
}