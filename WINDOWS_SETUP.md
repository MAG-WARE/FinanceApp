# 🪟 Guia de Setup para Windows - FinanceApp

## 🚀 Setup Rápido (Opção 1 - Recomendado)

Execute o script PowerShell automatizado:

```powershell
# No PowerShell, na raiz do projeto FinanceApp
.\quick-start.ps1
```

Se houver erro de política de execução, execute primeiro:
```powershell
Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope CurrentUser
```

Depois execute a API:
```powershell
cd FinanceApp.API
dotnet run
```

---

## 🔧 Setup Manual (Opção 2)

Se preferir executar passo a passo:

### 1. Limpar e Restaurar

```powershell
# Limpar projeto
dotnet clean

# Restaurar pacotes NuGet
dotnet restore

# Compilar
dotnet build
```

### 2. Instalar dotnet-ef (se não tiver)

```powershell
dotnet tool install --global dotnet-ef
```

### 3. Verificar PostgreSQL

Certifique-se que o PostgreSQL está instalado e rodando:

```powershell
# Verificar se o serviço está rodando (como administrador)
Get-Service postgresql*

# Ou verificar via pg_isready
pg_isready
```

Se não tiver PostgreSQL instalado:
1. Baixe em: https://www.postgresql.org/download/windows/
2. Instale com senha padrão: `postgres`
3. Crie o banco:

```powershell
# Conectar ao PostgreSQL
psql -U postgres

# No prompt do psql:
CREATE DATABASE financeapp;
\q
```

### 4. Criar e Aplicar Migrations

```powershell
# Criar migration inicial
dotnet ef migrations add InitialCreate --project FinanceApp.Infrastructure --startup-project FinanceApp.API

# Aplicar migration ao banco
dotnet ef database update --project FinanceApp.Infrastructure --startup-project FinanceApp.API
```

### 5. Executar a API

```powershell
cd FinanceApp.API
dotnet run
```

### 6. Acessar o Swagger

Abra o navegador em: **https://localhost:5001/swagger**

---

## ✅ Verificar se está funcionando

Após executar `dotnet run`, você deve ver:

```
[12:00:00 INF] Starting FinanceApp API
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: https://localhost:5001
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: http://localhost:5000
info: Microsoft.Hosting.Lifetime[0]
      Application started. Press Ctrl+C to shut down.
```

---

## 🐛 Problemas Comuns no Windows

### Erro: "dotnet-ef não é reconhecido"

**Solução 1**: Adicione ao PATH
```powershell
# Adicionar ao PATH do usuário
$env:PATH += ";$env:USERPROFILE\.dotnet\tools"

# Ou feche e abra o PowerShell novamente
```

**Solução 2**: Use o caminho completo
```powershell
~\.dotnet\tools\dotnet-ef migrations add InitialCreate --project FinanceApp.Infrastructure --startup-project FinanceApp.API
```

### Erro: "Cannot connect to database"

**Verifique**:
1. PostgreSQL está rodando?
   ```powershell
   Get-Service postgresql*
   ```

2. Connection string está correta?
   - Abra `FinanceApp.API/appsettings.json`
   - Verifique: `"Host=localhost;Database=financeapp;Username=postgres;Password=postgres"`

3. Firewall bloqueando?
   - Temporariamente desabilite o firewall para testar

### Erro: "A network-related or instance-specific error"

O PostgreSQL pode estar na porta diferente. Tente:
```json
"DefaultConnection": "Host=localhost;Port=5432;Database=financeapp;Username=postgres;Password=sua_senha"
```

### Erro: "Script execution is disabled"

Execute como administrador:
```powershell
Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope CurrentUser
```

---

## 📝 Atalhos PowerShell Úteis

Crie aliases para comandos frequentes:

```powershell
# Adicione ao seu perfil PowerShell ($PROFILE)

# Função para restaurar e compilar
function Build-FinanceApp {
    dotnet clean
    dotnet restore
    dotnet build
}

# Função para executar API
function Run-FinanceApp {
    Set-Location FinanceApp.API
    dotnet run
}

# Função para aplicar migrations
function Update-Database {
    dotnet ef database update --project FinanceApp.Infrastructure --startup-project FinanceApp.API
}

# Usar:
# Build-FinanceApp
# Run-FinanceApp
# Update-Database
```

---

## 🎯 Próximos Passos Após Setup

1. ✅ API rodando em https://localhost:5001
2. 🧪 Abra o Swagger: https://localhost:5001/swagger
3. 📖 Siga o guia: [TESTING_GUIDE.md](TESTING_GUIDE.md)
4. 🎉 Comece a testar os endpoints!

---

## 💡 Dicas para Desenvolvimento no Windows

### 1. Use Windows Terminal
Melhor que PowerShell padrão:
- Download: Microsoft Store → "Windows Terminal"

### 2. Configure Git Bash (Opcional)
Se preferir usar bash scripts:
- Instale Git for Windows
- Use Git Bash ao invés de PowerShell

### 3. Use VSCode
Editor recomendado para .NET:
- Instale extensões: C#, C# Dev Kit

### 4. PostgreSQL GUI
Ferramentas visuais úteis:
- pgAdmin 4 (vem com PostgreSQL)
- DBeaver (gratuito)
- Azure Data Studio

---

## 🔍 Verificar Tudo Funcionando

Execute este comando após o setup:

```powershell
# Testar compilação
dotnet build

# Verificar migrations
dotnet ef migrations list --project FinanceApp.Infrastructure --startup-project FinanceApp.API

# Testar conexão com banco
dotnet ef database update --project FinanceApp.Infrastructure --startup-project FinanceApp.API --verbose
```

---

## ✅ Checklist de Setup Completo

- [ ] .NET 8 SDK instalado
- [ ] PostgreSQL instalado e rodando
- [ ] Banco `financeapp` criado
- [ ] dotnet-ef instalado globalmente
- [ ] Projeto compila sem erros
- [ ] Migration criada e aplicada
- [ ] API executando sem erros
- [ ] Swagger acessível em https://localhost:5001/swagger

---

**Tudo pronto! Agora você pode testar o backend seguindo o [TESTING_GUIDE.md](TESTING_GUIDE.md)** 🚀
