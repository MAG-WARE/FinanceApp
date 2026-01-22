# 🔐 Configuração de Segurança - FinanceApp

## ⚠️ Problema: Credenciais no GitHub

**NUNCA** faça commit de:
- ❌ Senhas de banco de dados
- ❌ Chaves JWT secretas
- ❌ API keys
- ❌ Tokens de acesso
- ❌ Qualquer informação sensível

---

## ✅ Solução: User Secrets + Environment Variables

### **Desenvolvimento Local**: User Secrets
### **Produção**: Environment Variables

---

## 🚀 Setup Rápido (Recomendado)

Execute o script automatizado:

```powershell
.\setup-secrets.ps1
```

Este script vai:
1. ✅ Inicializar User Secrets no projeto
2. ✅ Solicitar sua senha do PostgreSQL
3. ✅ Gerar uma chave JWT segura automaticamente
4. ✅ Configurar todos os secrets necessários

---

## 🔧 Setup Manual

Se preferir configurar manualmente:

### **1. Inicializar User Secrets**

```powershell
cd FinanceApp.API
dotnet user-secrets init
```

### **2. Configurar Connection String**

```powershell
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Database=financeapp;Username=postgres;Password=SUA_SENHA_AQUI"
```

### **3. Gerar Chave JWT Segura**

Gere uma chave segura com PowerShell:

```powershell
# Gerar chave aleatória de 64 bytes (Base64)
$bytes = New-Object byte[] 64
[System.Security.Cryptography.RandomNumberGenerator]::Create().GetBytes($bytes)
$key = [Convert]::ToBase64String($bytes)
Write-Host $key
```

Copie a chave gerada e execute:

```powershell
dotnet user-secrets set "JwtSettings:SecretKey" "SUA_CHAVE_GERADA_AQUI"
```

### **4. Configurar Outros Settings JWT**

```powershell
dotnet user-secrets set "JwtSettings:Issuer" "FinanceApp"
dotnet user-secrets set "JwtSettings:Audience" "FinanceAppUsers"
dotnet user-secrets set "JwtSettings:ExpirationInMinutes" "1440"
```

---

## 🔍 Verificar Secrets Configurados

```powershell
cd FinanceApp.API
dotnet user-secrets list
```

Resultado esperado:
```
ConnectionStrings:DefaultConnection = Host=localhost;Database=...
JwtSettings:SecretKey = AbCd1234...
JwtSettings:Issuer = FinanceApp
JwtSettings:Audience = FinanceAppUsers
JwtSettings:ExpirationInMinutes = 1440
```

---

## 📁 Estrutura de Configuração

### **appsettings.Template.json** (Template - vai para GitHub)
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=financeapp;Username=postgres;Password=YOUR_PASSWORD_HERE"
  },
  "JwtSettings": {
    "SecretKey": "GENERATE_A_SECURE_KEY"
  }
}
```

### **User Secrets** (Local - NÃO vai para GitHub)
Armazenado em:
- **Windows**: `%APPDATA%\Microsoft\UserSecrets\<user-secrets-id>\secrets.json`
- **Linux/Mac**: `~/.microsoft/usersecrets/<user-secrets-id>/secrets.json`

---

## 🌍 Produção: Environment Variables

Em produção, use variáveis de ambiente:

### **Windows (IIS)**
```xml
<configuration>
  <system.webServer>
    <aspNetCore>
      <environmentVariables>
        <environmentVariable name="ConnectionStrings__DefaultConnection" value="Host=...;Password=..." />
        <environmentVariable name="JwtSettings__SecretKey" value="..." />
      </environmentVariables>
    </aspNetCore>
  </system.webServer>
</configuration>
```

### **Linux (systemd)**
```bash
# /etc/systemd/system/financeapp.service
[Service]
Environment="ConnectionStrings__DefaultConnection=Host=...;Password=..."
Environment="JwtSettings__SecretKey=..."
```

### **Docker**
```bash
docker run -e "ConnectionStrings__DefaultConnection=..." \
           -e "JwtSettings__SecretKey=..." \
           financeapp
```

### **Azure App Service**
Configure via Portal:
1. App Service → Configuration → Application settings
2. Adicione cada variável com formato: `ConnectionStrings__DefaultConnection`

### **AWS (Elastic Beanstalk)**
```bash
eb setenv ConnectionStrings__DefaultConnection="..." \
         JwtSettings__SecretKey="..."
```

---

## 🔄 Ordem de Precedência (ASP.NET Core)

A configuração é carregada nesta ordem (última sobrescreve):

1. ⬜ appsettings.json
2. ⬜ appsettings.{Environment}.json
3. ⬜ User Secrets (Development)
4. ⬜ Environment Variables
5. ✅ Command-line arguments (maior prioridade)

---

## 🗑️ Remover Secrets

Se precisar resetar:

```powershell
cd FinanceApp.API

# Remover todos os secrets
dotnet user-secrets clear

# Remover um secret específico
dotnet user-secrets remove "ConnectionStrings:DefaultConnection"
```

---

## 🆘 Troubleshooting

### ❌ "User secrets are not initialized"

**Solução**:
```powershell
cd FinanceApp.API
dotnet user-secrets init
```

### ❌ "Cannot find user secrets"

Verifique se o arquivo `.csproj` tem:
```xml
<PropertyGroup>
  <UserSecretsId>guid-aqui</UserSecretsId>
</PropertyGroup>
```

### ❌ "Connection string not found"

1. Verifique se configurou os secrets:
   ```powershell
   dotnet user-secrets list
   ```

2. Se vazio, execute:
   ```powershell
   .\setup-secrets.ps1
   ```

---

## ✅ Checklist de Segurança

Antes de fazer commit:

- [ ] ✅ `.gitignore` inclui `appsettings.json` e `appsettings.Development.json`
- [ ] ✅ `appsettings.Template.json` não contém dados sensíveis
- [ ] ✅ User Secrets configurados localmente
- [ ] ✅ Verificar que nenhum secret está sendo commitado:
  ```powershell
  git status
  git diff
  ```

---

## 📖 Referências

- [ASP.NET Core User Secrets](https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets)
- [Configuration in ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/configuration/)
- [Environment Variables](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/configuration/#environment-variables)

---

## 🎉 Resultado

✅ Credenciais seguras localmente
✅ Nada de sensível no GitHub
✅ Fácil deploy em produção
✅ Time pode colaborar sem compartilhar senhas

**Segurança garantida!** 🔐
