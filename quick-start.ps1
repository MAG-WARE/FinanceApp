# 🚀 FinanceApp - Quick Start Script for Windows PowerShell
# Este script prepara e executa o backend no Windows

Write-Host "======================================" -ForegroundColor Cyan
Write-Host "🚀 FinanceApp Backend - Quick Start" -ForegroundColor Cyan
Write-Host "======================================" -ForegroundColor Cyan
Write-Host ""

# Função para verificar status
function Check-Status {
    param([string]$Message)
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✅ $Message" -ForegroundColor Green
    } else {
        Write-Host "❌ Erro: $Message" -ForegroundColor Red
        exit 1
    }
}

# Passo 1: Limpar projeto
Write-Host "📦 Passo 1: Limpando projeto..." -ForegroundColor Yellow
dotnet clean > $null 2>&1
Check-Status "Projeto limpo"

# Passo 2: Restaurar pacotes
Write-Host "📦 Passo 2: Restaurando pacotes NuGet..." -ForegroundColor Yellow
dotnet restore
Check-Status "Pacotes restaurados"

# Passo 3: Compilar
Write-Host "🔨 Passo 3: Compilando projeto..." -ForegroundColor Yellow
dotnet build --no-restore
Check-Status "Projeto compilado com sucesso"

# Passo 4: Verificar se dotnet-ef está instalado
Write-Host "🔧 Passo 4: Verificando dotnet-ef..." -ForegroundColor Yellow
$efInstalled = dotnet tool list -g | Select-String "dotnet-ef"
if (-not $efInstalled) {
    Write-Host "dotnet-ef não encontrado. Instalando..." -ForegroundColor Yellow
    dotnet tool install --global dotnet-ef
    Check-Status "dotnet-ef instalado"
} else {
    Write-Host "✅ dotnet-ef já está instalado" -ForegroundColor Green
}

# Passo 5: Criar migration (se não existir)
Write-Host "🗄️  Passo 5: Verificando migrations..." -ForegroundColor Yellow
if (-not (Test-Path "FinanceApp.Infrastructure\Migrations")) {
    Write-Host "Criando migration inicial..." -ForegroundColor Yellow
    dotnet ef migrations add InitialCreate --project FinanceApp.Infrastructure --startup-project FinanceApp.API
    Check-Status "Migration criada"
} else {
    Write-Host "✅ Migrations já existem" -ForegroundColor Green
}

# Passo 6: Aplicar migration
Write-Host "🗄️  Passo 6: Aplicando migrations ao banco..." -ForegroundColor Yellow
dotnet ef database update --project FinanceApp.Infrastructure --startup-project FinanceApp.API
Check-Status "Banco de dados atualizado"

Write-Host ""
Write-Host "======================================" -ForegroundColor Green
Write-Host "✅ Setup completo!" -ForegroundColor Green
Write-Host "======================================" -ForegroundColor Green
Write-Host ""
Write-Host "Para executar a API:" -ForegroundColor Yellow
Write-Host "  cd FinanceApp.API" -ForegroundColor White
Write-Host "  dotnet run" -ForegroundColor White
Write-Host ""
Write-Host "Swagger UI estará disponível em:" -ForegroundColor Yellow
Write-Host "  https://localhost:5001/swagger" -ForegroundColor White
Write-Host ""
Write-Host "Leia o guia de testes em:" -ForegroundColor Yellow
Write-Host "  TESTING_GUIDE.md" -ForegroundColor White
Write-Host ""
