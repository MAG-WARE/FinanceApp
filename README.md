# FinanceApp - Sistema de Gestão Financeira Pessoal

## 🚀 Tecnologias

- **.NET 8** - Framework principal
- **Entity Framework Core 8** - ORM
- **PostgreSQL** - Banco de dados
- **JWT** - Autenticação
- **AutoMapper** - Mapeamento de objetos
- **FluentValidation** - Validações
- **Serilog** - Logging

## 📁 Estrutura do Projeto

```
FinanceApp/
├── FinanceApp.API/              # Controllers e configurações da API
├── FinanceApp.Application/      # DTOs, Services, Interfaces
├── FinanceApp.Domain/          # Entidades e Enums
└── FinanceApp.Infrastructure/  # DbContext, Repositories, Migrations
```

## 🎯 Funcionalidades

- ✅ Autenticação JWT
- ✅ Gestão de usuários
- ✅ Gestão de contas bancárias
- ✅ Categorização de transações
- ✅ Lançamento de receitas e despesas
- ✅ Orçamento mensal por categoria
- ✅ Metas financeiras
- ✅ Relatórios e dashboards

## ⚙️ Configuração

### 1. Banco de Dados
Configure a string de conexão do PostgreSQL no arquivo `appsettings.Development.json`

### 2. JWT Secret Key
O projeto requer uma chave secreta JWT para autenticação. Configure no arquivo `appsettings.Development.json`:

```json
{
  "JwtSettings": {
    "SecretKey": "SuaChaveSecretaComMinimo32Caracteres",
    "Issuer": "FinanceApp",
    "Audience": "FinanceAppUsers",
    "ExpirationInMinutes": 1440
  }
}
```

**Notas Importantes:**
- A SecretKey deve ter no mínimo 32 caracteres
- Use uma chave forte e única para cada ambiente
- Nunca commite o arquivo `appsettings.Development.json` (já está no .gitignore)
- Para produção, use variáveis de ambiente ou Azure Key Vault

### 3. Executar Migrations
```bash
cd FinanceApp.Infrastructure
dotnet ef database update
```

## 🤝 Contribuindo

Desenvolvido por Marcos - MAG.WARE

## 📄 Licença

Este projeto é de uso pessoal.
