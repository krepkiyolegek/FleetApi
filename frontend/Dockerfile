# Используем Nginx на базе Alpine Linux (весит всего ~10 МБ, минимальная поверхность для атак)
FROM nginx:alpine

# Копируем наш HTML файл в стандартную директорию Nginx
COPY index.html /usr/share/nginx/html/

# Открываем 80 порт внутри контейнера
EXPOSE 80

# Nginx запускается автоматически, ENTRYPOINT не нужен