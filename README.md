# CSharpTimescaleAPI

# Требования

- .NET 8 SDK

- Docker (для PostgreSQL)

## Запуск

1. Запустить PostgreSQL в Docker:

```bash

docker run --name postgres-timescale -e POSTGRES\_PASSWORD=mysecretpassword -e POSTGRES\_DB=timescale\_db -p 5432:5432 -d postgres:16

```

2. Настроить User Secrets (или использовать appsettings.json):

```bash

dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Database=timescale\_db;Username=postgres;Password=mysecretpassword"

```

3. Выполнить миграции:

```bash

dotnet ef database update

```

4. Запустить приложение:

```bash

dotnet run

```

5. Открыть Swagger:

```

https://localhost:5001/swagger

```

## Тестирование

```bash

dotnet test

```