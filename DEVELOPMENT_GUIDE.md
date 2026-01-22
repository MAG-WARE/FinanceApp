# 🛠️ Guia de Desenvolvimento - FinanceApp

## ✅ O que já está pronto

### Estrutura do projeto
- ✅ Solution com 4 projetos (API, Application, Domain, Infrastructure)
- ✅ Entities completas (User, Account, Category, Transaction, Budget, Goal)
- ✅ DbContext configurado com relacionamentos
- ✅ Enums (AccountType, TransactionType, CategoryType)
- ✅ DTOs básicos (Auth, Account, Transaction)
- ✅ Repository pattern base
- ✅ Configuração JWT
- ✅ Serilog para logs
- ✅ CORS configurado
- ✅ Swagger habilitado

### Configurações
- ✅ Connection string PostgreSQL
- ✅ JWT settings
- ✅ Logging configurado
- ✅ launchSettings.json

## 🚧 Próximos passos (use Claude Code para isso)

### 1. Implementar Autenticação
- [ ] AuthService com hash de senha (BCrypt)
- [ ] Geração de tokens JWT
- [ ] AuthController (Register, Login)
- [ ] Middleware de autenticação

### 2. Implementar Services e Repositories
- [ ] IAccountRepository e AccountRepository
- [ ] AccountService
- [ ] AccountController (CRUD completo)
- [ ] ICategoryRepository e CategoryRepository
- [ ] CategoryService
- [ ] CategoryController
- [ ] ITransactionRepository e TransactionRepository
- [ ] TransactionService (incluir cálculo de saldo)
- [ ] TransactionController

### 3. Validações
- [ ] FluentValidation para DTOs
- [ ] Validações de negócio nos Services
- [ ] Exception handling middleware

### 4. Features avançadas
- [ ] BudgetService e Controller
- [ ] GoalService e Controller
- [ ] DashboardService (estatísticas, gráficos)
- [ ] ReportService (relatórios mensais, anuais)

### 5. Melhorias
- [ ] Unit tests
- [ ] Paginação nas listagens
- [ ] Filtros e ordenação
- [ ] Soft delete implementado
- [ ] Audit trail (CreatedBy, UpdatedBy)
- [ ] Cache (Redis opcional)

## 📝 Comandos importantes

### Primeira vez - Criar banco
```bash
cd FinanceApp.API
dotnet ef migrations add InitialCreate --project ../FinanceApp.Infrastructure
dotnet ef database update
```

### Rodar aplicação
```bash
dotnet run --project FinanceApp.API
```

### Acessar Swagger
Após rodar, acesse: `https://localhost:5001/swagger`

## 🎯 Ordem sugerida de implementação com Claude Code

1. **Autenticação primeiro**
   - Implemente AuthService e AuthController
   - Teste registro e login
   - Valide tokens JWT

2. **Accounts (Contas)**
   - Implemente CRUD completo
   - Adicione cálculo de saldo atual
   - Teste com Swagger

3. **Categories (Categorias)**
   - CRUD básico
   - Categorias default ao criar usuário

4. **Transactions (Transações)**
   - CRUD completo
   - Validar se conta/categoria pertence ao usuário
   - Recalcular saldo da conta após transação
   - Implementar transferências entre contas

5. **Dashboard e Relatórios**
   - Endpoints para estatísticas
   - Gastos por categoria
   - Evolução mensal

6. **Budget e Goals**
   - Implementar depois da base estar sólida

## 💡 Dicas

- Use `[Authorize]` nos controllers após implementar auth
- Todo endpoint deve validar se o recurso pertence ao usuário logado
- Use AutoMapper para mapear entities <-> DTOs
- Configure FluentValidation para validar requests
- Mantenha as regras de negócio nos Services, não nos Controllers

## 🔐 Segurança

- Altere o JWT SecretKey no appsettings.json
- Use variáveis de ambiente em produção
- Implemente rate limiting
- Valide sempre se o usuário tem permissão para acessar o recurso

## 📊 Exemplo de fluxo completo

1. User se registra (POST /api/auth/register)
2. User faz login (POST /api/auth/login) - recebe token
3. User cria uma conta (POST /api/accounts) - passa token no header
4. User cria categorias (POST /api/categories)
5. User lança transações (POST /api/transactions)
6. User consulta dashboard (GET /api/dashboard)

## 🐛 Debug

Se der erro de migration:
```bash
# Remove a migration
dotnet ef migrations remove --project FinanceApp.Infrastructure --startup-project FinanceApp.API

# Recria
dotnet ef migrations add InitialCreate --project FinanceApp.Infrastructure --startup-project FinanceApp.API
```

Se der erro de conexão com PostgreSQL:
- Verifique se o PostgreSQL está rodando
- Confira a connection string no appsettings
- Teste a conexão com pgAdmin ou DBeaver

## 🎨 Frontend (depois)

Após a API estar funcionando, você pode criar o frontend com Next.js seguindo a estrutura:
```
frontend/
└── finance-web/
    ├── src/
    │   ├── app/          # Pages (App Router)
    │   ├── components/   # Componentes React
    │   ├── services/     # API calls
    │   ├── types/        # TypeScript types
    │   └── lib/          # Utils, config
    └── public/
```

Boa sorte com o desenvolvimento! 🚀
