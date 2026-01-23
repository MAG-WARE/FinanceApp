# FinanceApp Configuration Setup Script
Write-Host "=========================================" -ForegroundColor Cyan
Write-Host "  FinanceApp - Configuration Setup" -ForegroundColor Cyan
Write-Host "=========================================" -ForegroundColor Cyan
Write-Host ""

# Check if appsettings.Development.json exists
$configPath = "FinanceApp.API/appsettings.Development.json"
if (Test-Path $configPath) {
    Write-Host "✓ appsettings.Development.json already exists" -ForegroundColor Yellow
    $overwrite = Read-Host "Do you want to overwrite it? (y/n)"
    if ($overwrite -ne 'y' -and $overwrite -ne 'Y') {
        Write-Host "Setup cancelled." -ForegroundColor Red
        exit 0
    }
}

# Generate a random JWT secret key (64 characters)
$bytes = New-Object byte[] 48
$rng = [System.Security.Cryptography.RandomNumberGenerator]::Create()
$rng.GetBytes($bytes)
$jwtSecret = [Convert]::ToBase64String($bytes)

# Create appsettings.Development.json
$config = @{
    Logging = @{
        LogLevel = @{
            Default = "Debug"
            "Microsoft.AspNetCore" = "Information"
        }
    }
    ConnectionStrings = @{
        DefaultConnection = "Host=localhost;Database=financeapp_dev;Username=postgres;Password=YOUR_PASSWORD_HERE"
    }
    JwtSettings = @{
        SecretKey = $jwtSecret
        Issuer = "FinanceApp"
        Audience = "FinanceAppUsers"
        ExpirationInMinutes = 1440
    }
    Serilog = @{
        Using = @("Serilog.Sinks.Console")
        MinimumLevel = @{
            Default = "Debug"
            Override = @{
                Microsoft = "Warning"
                System = "Warning"
            }
        }
        WriteTo = @(
            @{
                Name = "Console"
                Args = @{
                    outputTemplate = "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}"
                }
            }
        )
    }
}

$config | ConvertTo-Json -Depth 10 | Set-Content -Path $configPath

Write-Host ""
Write-Host "✓ Configuration file created successfully!" -ForegroundColor Green
Write-Host ""
Write-Host "⚠️  IMPORTANT: Update the database password in:" -ForegroundColor Yellow
Write-Host "   FinanceApp.API/appsettings.Development.json" -ForegroundColor Yellow
Write-Host ""
Write-Host "JWT Secret Key has been generated automatically." -ForegroundColor Green
Write-Host ""
Write-Host "Next steps:" -ForegroundColor Cyan
Write-Host "1. Update the PostgreSQL password in appsettings.Development.json"
Write-Host "2. Run migrations: cd FinanceApp.Infrastructure; dotnet ef database update"
Write-Host "3. Start the application: cd FinanceApp.API; dotnet run"
Write-Host ""
