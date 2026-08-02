using FleetApi.Models;
using FleetApi.Data;
using FleetApi.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace FleetApi.Controllers;

// Попасть в данный контроллер можно только при наличии валидного токена
[Authorize]
// [ApiController] — говорит системе, что этот класс отвечает на HTTP-запросы и автоматически проверяет валидность DTO (ModelState).
[ApiController]
// [Route("api/[controller]")] — задает адрес. Если класс называется CarsController, адрес будет /api/cars.
[Route("api/[controller]")]
public class CarsController: ControllerBase
{
    // Ссылка на контекст в базе данных. readonly - чтобы не подменить ее случайно в коде
    private readonly AppDbContext _context;

    // Конструктор: Program.cs сам передаст сюда AppDbContext (внедрение зависимости)
    public CarsController(AppDbContext context)
    {
        _context = context;
    }

    // Async/Await: Мы везде используем асинхронность. Это позволяет серверу не "зависать" в ожидании ответа от базы данных, 
    // а обрабатывать другие запросы. Для высоконагруженных систем это критично.

    // Получение всех машин
    [HttpGet]
    // ActionResult: Это "обертка". Она позволяет возвращать как сами данные (Car), так и HTTP-статусы (NotFound, BadRequest).
    public async Task<ActionResult<IEnumerable<Car>>> GetCars()
    {
        // .AsNoTracking() — БЕЗОПАСНОСТЬ И ОПТИМИЗАЦИЯ: говорим базе, что мы только читаем. 
        // Это ускоряет работу и экономит память, так как EF не будет следить за изменениями этих объектов.
        return await _context.Cars.AsNoTracking().ToListAsync();
    }

    // Получение одной машины по id
    [HttpGet ("{id}")]
    public async Task<ActionResult<Car>> GetCar(int id)
    {
        // Ищем машину в базе по первичному ключу
        var car = await _context.Cars.FindAsync(id);

        // Если не нашли возвращаем 404 Not Found
        if (car == null) return NotFound();

        return car;
    }

    // Создание новой машины
    [HttpPost]
    public async Task<ActionResult<Car>> CreateCar(CarCreateUpdateDto dto)
    {
        // Безопасность: Проверяем нет ли уже машин с таким госномером.
        if(await _context.Cars.AnyAsync(c => c.RegistrationNumber == dto.RegistrationNumber))
        {
            return BadRequest("Автомобиль с таким госномером уже существует.");
        }

        // Безопасность (Mass Assignment): Мы вручную перекладываем данные из DTO в Модель
        // Пользователь не сможет изменить поля, которых нет в DTO (например, какой-нибудь IsAdmin или HiddenNotes)
        var car = new Car
        {
            Make = dto.Make,
            Model = dto.Model,
            Year = dto.Year,
            RegistrationNumber = dto.RegistrationNumber,
            // ToUniversalTime(): PostgreSQL очень строг к часовым поясам. Всегда сохраняй даты в формате UTC, 
            // чтобы избежать путаницы со временем на разных серверах.
            LastInspectionDate = dto.LastInspectionDate.ToUniversalTime()
        };

        // Добавляем объект в очередь на сохранение
        _context.Cars.Add(car);
        // Выполняем SQL-запрос INSERT
        await _context.SaveChangesAsync();

        // Возвращаем статус 201 Created и ссылку на созданный объект
        // nameof(GetCar) Мы говорим фреймворку: "Чтобы найти эту новую машину, используй метод GetCar".
        // Архитектурный совет: Мы используем nameof(GetCar), а не просто пишем строку "GetCar". Если в будущем ты переименуешь 
        // метод в GetVehicle, Visual Studio сама переименует его и здесь. Строка бы так и осталась "GetCar", и код бы сломался.
        // new { id = car.Id } Методу GetCar для работы нужен id. Здесь мы передаем ему тот самый ID, 
        // который база данных только что сгенерировала для новой машины (например, 5).
        // car Это сами данные (JSON), которые мы отправляем обратно клиенту в теле ответа. Фронтенд-разработчик скажет тебе 
        // спасибо: ему не придется делать дополнительный GET запрос, чтобы показать добавленную машину на экране.
        return CreatedAtAction(nameof(GetCar), new { id = car.Id }, car);
    }

    // Обновление данных
    [HttpPut ("{id}")]
    // Обещаешь вернуть данные (JSON) — используй ActionResult<Тип>.
    // Возвращаешь только статус-коды без тела ответа — используй IActionResult
    public async Task<IActionResult> UpdateCar(int id, CarCreateUpdateDto dto)
    {
        // Ищем машину в базе по первичному ключу
        var car = await _context.Cars.FindAsync(id);

        // Если не нашли возвращаем 404 Not Found
        if (car == null) return NotFound();

        // Обновляем только разрешенные поля из DTO
        car.Make = dto.Make;
        car.Model = dto.Model;
        car.Year = dto.Year;
        car.RegistrationNumber = dto.RegistrationNumber;
        car.LastInspectionDate = dto.LastInspectionDate.ToUniversalTime();

        // Сохраняем измененные данные
        await _context.SaveChangesAsync();

        // Возвращаем 204 No Content (успешно, но данных в ответе нет)
        return NoContent();
    }

    // Удаление
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCar(int id)
    {
        // Ищем машину в базе по первичному ключу
        var car = await _context.Cars.FindAsync(id);

        // Если не нашли возвращаем 404 Not Found
        if (car == null) return NotFound();

        // Помечаем объект на удаление
        _context.Cars.Remove(car);
        // Выполняем SQL Delete
        await _context.SaveChangesAsync();

        return NoContent();        
    }
}