using FleetApi.Models;
using Microsoft.EntityFrameworkCore;

namespace FleetApi.Data;

// Наследуемся от DbContext. Тем самым мы говорим фреймворку: 
// "Этот класс — наш главный шлюз к базе данных".
public class AppDbContext : DbContext
{
    // Конструктор. Сюда из Program.cs прилетают настройки подключения 
    // (логин, пароль, адрес сервера). `base(options)` передает их в базовый класс.
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    // DbSet — это специальный класс в Entity Framework. Он переводится как "Набор данных". 
    // Указывая <Car>, ты говоришь фреймворку: "Я хочу, чтобы в базе данных была таблица, структура которой полностью 
    // совпадает с классом Car" 
    // Cars Это имя свойства. Именно так будет называться таблица в PostgreSQL. 
    // Если бы ты написал public DbSet<Car> Automobiles, то EF Core создал бы в базе таблицу Automobiles.
    // => Set<Car>() Это сокращенный синтаксис C# (называется Expression Body). По сути, это безопасный способ инициализации.
    // Метод Set<Car>() встроен в базовый класс DbContext. Он связывает твое свойство Cars с внутренними механизмами EF Core.
    public DbSet<Car> Cars => Set<Car>();
    public DbSet<User> Users => Set<User>();

    // 4. Этот метод срабатывает один раз, когда фреймворк строит структуру БД.
    // Здесь мы прописываем жесткие правила для базы данных.
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Правило безопасности и целостности данных:
        // Мы говорим базе данных создать Индекс по полю RegistrationNumber 
        // и сделать его уникальным (.IsUnique()).
        // Не получиться у двоих пользователей одновременно создать объект с одинаковым регистрационным номером
        modelBuilder.Entity<Car>()
            .HasIndex(c => c.RegistrationNumber)
            .IsUnique();

        // Имя пользователя должно быть уникальным
        modelBuilder.Entity<User>()
            .HasIndex(u => u.Username)
            .IsUnique();
    }
}