using FleetApi.Data;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.OpenApi;

// Создаем "строителя" приложения. Он читает настройки (appsettings.json, секреты)
var builder = WebApplication.CreateBuilder(args);

// --- ЧАСТЬ 1: РЕГИСТРАЦИЯ СЕРВИСОВ (Инструменты для завода) ---

// БЕЗОПАСНОСТЬ: Настраиваем политику CORS (разрешаем запросы откуда угодно для разработки)
builder.Services.AddCors(options =>
{
    // Создаем свод правил и называем его "AllowAll" (Разрешить всё)
    options.AddPolicy("AllowAll", policy =>
    {
        // Мы написали .AllowAnyOrigin() только для локальной разработки, чтобы не мучиться с портами. 
        // В реальном продакшене (на боевом сервере) так делать КАТЕГОРИЧЕСКИ НЕЛЬЗЯ. 
        // Там ты должен будешь жестко прописать домен твоего фронтенда, например:
        // policy.WithOrigins("https://my-fleet-dashboard.com"). Иначе любой злоумышленник сможет встроить запросы 
        // к твоему API на своем сайте.
        policy.AllowAnyOrigin() // 1. Разрешаем запросы с ЛЮБЫХ доменов/портов.
        .AllowAnyMethod() // 2. Разрешаем ЛЮБЫЕ методы (GET, POST, PUT, DELETE).
        .AllowAnyHeader(); // 3. Разрешаем ЛЮБЫЕ заголовки (например, Content-Type или токены авторизации).
    });
});

// Безопасное получение строки подключения из secrets
var connectionSrting = builder.Configuration.GetConnectionString("DefaultConnection");

// Регистрируем наш AppDbContext. 
// Теперь, когда контроллеру понадобится база, фреймворк сам создаст контекст 
// и передаст его в контроллер. Это паттерн "Внедрение зависимостей" (Dependency Injection).
builder.Services.AddDbContext<AppDbContext>(options => options.UseNpgsql(connectionSrting));

// Настройка проверки JWT(JSON Web Token)
var jwtKey = builder.Configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key is missing");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true, // Проверять, кто выдал токен
            ValidateAudience = false, // Пока не проверяем, для какого клиента(сервиса) выдан токен
            ValidateLifetime = true, // Проверять, не истек ли срок действия
            ValidateIssuerSigningKey = true, // Самое важное: проверять криптографическую подпись
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });
builder.Services.AddAuthorization();

// Учим приложение понимать архитектуру контроллеров (чтобы работали [ApiController])
builder.Services.AddControllers();

// Подключаем генерацию документации API 
builder.Services.AddOpenApi();

// Завершаем сборку сервисов и создаем само веб-приложение
var app = builder.Build();

// --- ЧАСТЬ 2: НАСТРОЙКА КОНВЕЙЕРА ЗАПРОСОВ (Middlewares / Проходная) ---
// Порядок строк здесь КРИТИЧЕСКИ ВАЖЕН! Запрос проходит их сверху вниз.

// --- СТАРТ БЛОКА АВТОМАТИЧЕСКОЙ МИГРАЦИИ ---
// Создаем временную область видимости (scope) для получения сервисов
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        // Достаем наш контекст базы данных
        var context = services.GetRequiredService<AppDbContext>();
        // Команда Migrate() делает то же самое, что и 'dotnet ef database update',
        // но выполняется самим приложением внутри Docker-контейнера.
        context.Database.Migrate(); 
        Console.WriteLine("Миграции успешно применены к базе данных.");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Ошибка при применении миграций: {ex.Message}");
    }
}

// БЕЗОПАСНОСТЬ: Если мы запускаем код на локальном компе (Development)
if (app.Environment.IsDevelopment())
{
    // .NET 9 СТАНДАРТ: Создаем файл документации и подключаем современный UI
    app.MapOpenApi();
    app.MapScalarApiReference(); // Интерфейс будет доступен по адресу /scalar
}

// БЕЗОПАСНОСТЬ: Если клиент обратился по небезопасному HTTP, 
// принудительно перенаправляем его на зашифрованный HTTPS. Защита от перехвата трафика.
//app.UseHttpsRedirection(); 

// ВАЖНО: Эта строка должна быть ДО app.UseAuthorization() и app.MapControllers()
app.UseCors("AllowAll");

// Спрашиваем паспорт (Кто ты?)
app.UseAuthentication();

// БЕЗОПАСНОСТЬ: Подключаем проверку прав доступа. 
// Пока у нас нет логинов/паролей, но этот "охранник" уже стоит на посту для будущих задач.
app.UseAuthorization();

// Указываем приложению, что нужно сопоставить входящие URL-адреса с нашими Контроллерами
// Пускаем к данным
app.MapControllers();

// Запускаем сервер. Приложение начинает слушать входящие запросы.
app.Run();