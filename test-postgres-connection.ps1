# 🔍 Script para testar conexão com PostgreSQL
Write-Host "=== Teste de Conexão PostgreSQL ===" -ForegroundColor Cyan

# Tentar senhas comuns
$senhas = @("postgres", "admin", "root", "123456", "password")

Write-Host "`nTestando senhas comuns..." -ForegroundColor Yellow

foreach ($senha in $senhas) {
    Write-Host "`nTestando senha: $senha" -ForegroundColor Gray
    $env:PGPASSWORD = $senha

    $resultado = psql -U postgres -c "SELECT version();" 2>&1

    if ($LASTEXITCODE -eq 0) {
        Write-Host "✅ SUCESSO! A senha é: $senha" -ForegroundColor Green
        Write-Host "`nAtualize seu appsettings.json com:" -ForegroundColor Cyan
        Write-Host '"DefaultConnection": "Host=localhost;Database=financeapp;Username=postgres;Password=' + $senha + '"' -ForegroundColor White
        exit 0
    }
}

Write-Host "`n❌ Nenhuma senha comum funcionou." -ForegroundColor Red
Write-Host "`nOpções:" -ForegroundColor Yellow
Write-Host "1. Tente redefinir a senha via pgAdmin" -ForegroundColor White
Write-Host "2. Edite pg_hba.conf temporariamente para 'trust'" -ForegroundColor White
Write-Host "3. Digite a senha manualmente:" -ForegroundColor White

$senhaManual = Read-Host "Digite a senha do PostgreSQL (ou Enter para pular)"

if ($senhaManual) {
    $env:PGPASSWORD = $senhaManual
    $resultado = psql -U postgres -c "SELECT version();" 2>&1

    if ($LASTEXITCODE -eq 0) {
        Write-Host "`n✅ SUCESSO! Senha correta!" -ForegroundColor Green
        Write-Host "`nAtualize seu appsettings.json com:" -ForegroundColor Cyan
        Write-Host '"DefaultConnection": "Host=localhost;Database=financeapp;Username=postgres;Password=' + $senhaManual + '"' -ForegroundColor White
    } else {
        Write-Host "`n❌ Senha incorreta." -ForegroundColor Red
    }
}
