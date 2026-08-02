# Fleet Management System 🚗

Микросервисное Full-Stack приложение для управления автопарком. Разработано с упором на безопасность (JWT, хэширование паролей, изоляция секретов) и DevOps-практики.

## 🛠 Технологический стек
* **Backend:** C# / .NET 9, Web API, Entity Framework Core
* **Database:** PostgreSQL 15
* **Frontend:** Vanilla JS, HTML5, Bootstrap 5, Nginx
* **DevOps:** Docker, Docker Compose, GitHub Actions (CI/CD)
* **Security:** JWT Authentication, BCrypt (Password Hashing), CORS policies

## 🚀 Как запустить локально

1. Склонируйте репозиторий:
   ```bash
   git clone https://github.com/ВАШ_ЛОГИН/FleetApi.git
   cd FleetApi

2. Создайте файл .env в корне проекта и добавьте секреты:
   DB_PASSWORD=YourPassword
   JWT_SECRET_KEY=YourSecretKeyForJwtAuthentication

3. Запустите проект через Docker Compose:
   docker compose pull
   docker compose up -d

4. Откройте интерфейсы:
   Dashboard (Frontend): http://localhost:3000
   API Docs (Scalar/Swagger): http://localhost:8080/scalar
   Миграции базы данных применяются автоматически при запуске контейнера