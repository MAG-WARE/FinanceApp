# FinanceApp - Sistema de Gestão Financeira Pessoal

## 🚀 Tecnologias

- **.NET 8** - Framework principal
- **Entity Framework Core 8** - ORM
- **PostgreSQL** - Banco de dados
- **JWT** - Autenticação
- **AutoMapper** - Mapeamento de objetos
- **FluentValidation** - Validações
- **Serilog** - Logging

## 📋 Pré-requisitos

- .NET 8 SDK
- PostgreSQL 15+
- Visual Studio 2022 / JetBrains Rider / VS Code

## 🔧 Instalação

1. Clone o repositório
```bash
git clone [seu-repositorio]
cd FinanceApp/backend
```

2. Restaure os pacotes
```bash
dotnet restore
```

3. Configure o banco de dados no `appsettings.Development.json`
```json
"ConnectionStrings": {
  "DefaultConnection": "Host=localhost;Database=financeapp_dev;Username=seu_usuario;Password=sua_senha"
}
```

4. Crie o banco de dados
```bash
cd FinanceApp.API
dotnet ef migrations add InitialCreate --project ../FinanceApp.Infrastructure
dotnet ef database update
```

5. Execute a aplicação
```bash
dotnet run
```

A API estará disponível em `https://localhost:5001` e `http://localhost:5000`

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

## 📝 Comandos úteis

```bash
# Criar nova migration
dotnet ef migrations add NomeDaMigration --project FinanceApp.Infrastructure --startup-project FinanceApp.API

# Aplicar migrations
dotnet ef database update --project FinanceApp.Infrastructure --startup-project FinanceApp.API

# Remover última migration
dotnet ef migrations remove --project FinanceApp.Infrastructure --startup-project FinanceApp.API

# Build da solution
dotnet build

# Run da API
dotnet run --project FinanceApp.API
```

## 🔐 Configuração JWT

Altere a chave secreta no `appsettings.json` em produção:
```json
"JwtSettings": {
  "SecretKey": "sua-chave-super-secreta-aqui-minimo-32-caracteres",
  "Issuer": "FinanceApp",
  "Audience": "FinanceAppUsers",
  "ExpirationInMinutes": 1440
}
```

## 📊 Modelo de Dados

### User
- Informações do usuário
- Email único
- Senha hasheada

### Account
- Contas bancárias/carteiras
- Tipos: Corrente, Poupança, Carteira, Investimento, Cartão de Crédito
- Saldo inicial

### Category
- Categorias de receitas e despesas
- Personalizáveis por usuário

### Transaction
- Lançamentos financeiros
- Receitas, Despesas e Transferências
- Suporte a transações recorrentes

### Budget
- Orçamento mensal por categoria
- Acompanhamento de limites

### Goal
- Metas financeiras
- Acompanhamento de progresso

## 🤝 Contribuindo

Desenvolvido por Marcos - MAG.WARE

## 📄 Licença

Este projeto é de uso pessoal.
