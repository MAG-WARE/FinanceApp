# 🗄️ Script para criar banco de dados FinanceApp
# Execute este script no PowerShell

Write-Host "=== Criando Banco de Dados FinanceApp ===" -ForegroundColor Green

# Tentar conectar ao PostgreSQL e criar o banco
try {
    $env:PGPASSWORD = "postgres"

    Write-Host "Conectando ao PostgreSQL..." -ForegroundColor Yellow

    # Criar o banco de dados
    psql -U postgres -c "CREATE DATABASE financeapp;" 2>$null

    if ($LASTEXITCODE -eq 0) {
        Write-Host "✅ Banco 'financeapp' criado com sucesso!" -ForegroundColor Green
    } else {
        Write-Host "⚠️ Banco 'financeapp' já existe ou erro ao criar" -ForegroundColor Yellow
    }

    # Verificar se o banco foi criado
    Write-Host "`nBancos de dados disponíveis:" -ForegroundColor Cyan
    psql -U postgres -c "\l" | Select-String "financeapp"

    Write-Host "`n✅ Pronto! Agora você pode executar as migrations." -ForegroundColor Green
    Write-Host "Execute: dotnet ef migrations add InitialCreate --project FinanceApp.Infrastructure --startup-project FinanceApp.API" -ForegroundColor Cyan

} catch {
    Write-Host "❌ Erro ao criar banco de dados: $_" -ForegroundColor Red
    Write-Host "`nVerifique se o PostgreSQL está instalado e rodando." -ForegroundColor Yellow
    Write-Host "Instale em: https://www.postgresql.org/download/windows/" -ForegroundColor Yellow
}
