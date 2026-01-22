# 🔐 Script para configurar User Secrets no FinanceApp
# Este script configura credenciais locais sem expô-las no GitHub

Write-Host "=== Configurando User Secrets para FinanceApp ===" -ForegroundColor Cyan
Write-Host ""

# Função para gerar chave JWT segura
function Generate-JwtSecretKey {
    $bytes = New-Object byte[] 64
    $rng = [System.Security.Cryptography.RandomNumberGenerator]::Create()
    $rng.GetBytes($bytes)
    return [Convert]::ToBase64String($bytes)
}

# Navegar para o projeto API
Set-Location FinanceApp.API

# Inicializar User Secrets (se não existir)
Write-Host "Inicializando User Secrets..." -ForegroundColor Yellow
dotnet user-secrets init 2>$null

# Solicitar senha do PostgreSQL
Write-Host ""
Write-Host "Digite a senha do PostgreSQL:" -ForegroundColor Green
$postgresPassword = Read-Host -AsSecureString
$postgresPasswordPlain = [Runtime.InteropServices.Marshal]::PtrToStringAuto(
    [Runtime.InteropServices.Marshal]::SecureStringToBSTR($postgresPassword)
)

# Gerar chave JWT segura
Write-Host ""
Write-Host "Gerando chave JWT segura..." -ForegroundColor Yellow
$jwtSecretKey = Generate-JwtSecretKey

# Configurar User Secrets
Write-Host ""
Write-Host "Configurando secrets..." -ForegroundColor Yellow

dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Database=financeapp;Username=postgres;Password=$postgresPasswordPlain"
dotnet user-secrets set "JwtSettings:SecretKey" $jwtSecretKey
dotnet user-secrets set "JwtSettings:Issuer" "FinanceApp"
dotnet user-secrets set "JwtSettings:Audience" "FinanceAppUsers"
dotnet user-secrets set "JwtSettings:ExpirationInMinutes" "1440"

Write-Host ""
Write-Host "✅ User Secrets configurados com sucesso!" -ForegroundColor Green
Write-Host ""
Write-Host "📝 Secrets configurados:" -ForegroundColor Cyan
Write-Host "  - ConnectionString do PostgreSQL" -ForegroundColor White
Write-Host "  - JWT SecretKey (gerada automaticamente)" -ForegroundColor White
Write-Host "  - JWT Issuer: FinanceApp" -ForegroundColor White
Write-Host "  - JWT Audience: FinanceAppUsers" -ForegroundColor White
Write-Host "  - JWT Expiration: 24 horas" -ForegroundColor White
Write-Host ""
Write-Host "🔍 Para ver seus secrets:" -ForegroundColor Yellow
Write-Host "  dotnet user-secrets list" -ForegroundColor White
Write-Host ""
Write-Host "🗑️  Para remover todos os secrets:" -ForegroundColor Yellow
Write-Host "  dotnet user-secrets clear" -ForegroundColor White
Write-Host ""
Write-Host "✅ Agora você pode executar a aplicação sem expor credenciais!" -ForegroundColor Green

# Voltar ao diretório raiz
Set-Location ..
