# CSharpTimescaleAPI

# Требования

- .NET 8 SDK

- Docker (для PostgreSQL)

## Запуск

1. Запустить PostgreSQL в Docker:

```bash

docker run --name postgres-timescale -e POSTGRES_PASSWORD=mysecretpassword -e POSTGRES_DB=timescale_db -p 5432:5432 -d postgres:16

```

2. Настроить User Secrets (или использовать appsettings.json):

```bash

dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Database=timescale_db;Username=postgres;Password=mysecretpassword"

```

3. Установка инструментов

```bash
.\run.ps1 install
```

4. Выполнить миграции:

```bash

.\run.ps1 migrate

```

5. Запустить приложение:

```bash

.\run.ps1

```

6. Открыть Swagger:

```

https://localhost:7292/swagger/index.html

```

## Тестирование

```bash
.\run.ps1 test

```