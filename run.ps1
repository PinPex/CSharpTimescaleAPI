# run.ps1
param(
    [string]$Command = "run"
)


switch ($Command) {
    "run" {
        dotnet run --project CSharpTimescaleAPI
    }
    "install"{
        dotnet tool install --global dotnet-ef
    }
    "migrate" {
        dotnet ef database update --project CSharpTimescaleAPI
    }
    "test" {
        dotnet test
    }
    default {
        Write-Host "Unknown command. Use: run, migrate, test"
    }
}