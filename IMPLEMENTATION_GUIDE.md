# Guia de Implementação - FinanceApp Backend

## ✅ Implementação Completa

Todo o backend foi implementado com sucesso! Aqui está o que foi criado:

### 📦 Estrutura Criada

#### **Application Layer**
- ✅ **DTOs**: Auth, Account, Category, Transaction, Budget, Goal, Dashboard
- ✅ **Validators**: FluentValidation para todos os DTOs
- ✅ **Interfaces**: Todos os serviços (IAuthService, IAccountService, etc.)
- ✅ **Services**: Implementação completa de todos os serviços
- ✅ **Mappings**: AutoMapper profile configurado

#### **API Layer**
- ✅ **Controllers**: Auth, Account, Category, Transaction, Budget, Goal, Dashboard
- ✅ **Middleware**: Exception Handling global
- ✅ **Configuration**: Program.cs com injeção de dependências

#### **Funcionalidades Implementadas**

1. **Autenticação (AuthController)**
   - ✅ Registro com hash BCrypt
   - ✅ Login com JWT
   - ✅ Endpoint /me para validar token
   - ✅ Criação automática de categorias padrão no registro

2. **Contas (AccountController)**
   - ✅ CRUD completo
   - ✅ Cálculo automático de saldo (InitialBalance + transações)
   - ✅ Filtro por contas ativas
   - ✅ Validação de propriedade (usuário só vê suas contas)

3. **Categorias (CategoryController)**
   - ✅ CRUD completo
   - ✅ Filtro por tipo (Income/Expense)
   - ✅ 13 categorias padrão criadas no registro

4. **Transações (TransactionController)**
   - ✅ CRUD completo
   - ✅ Validação de conta e categoria
   - ✅ Suporte a transferências entre contas
   - ✅ Filtros: por conta, categoria, tipo, período
   - ✅ Paginação implementada

5. **Orçamentos (BudgetController)**
   - ✅ CRUD completo
   - ✅ Validação de orçamentos duplicados
   - ✅ Cálculo de gastos vs limite
   - ✅ Endpoint de status com percentual usado
   - ✅ Alerta quando ultrapassar orçamento

6. **Metas (GoalController)**
   - ✅ CRUD completo
   - ✅ Marcação automática como completa
   - ✅ Cálculo de progresso e valor restante
   - ✅ Filtros: ativas/completadas

7. **Dashboard (DashboardController)**
   - ✅ Resumo do mês atual
   - ✅ Top 5 categorias de gastos
   - ✅ Evolução dos últimos 6 meses
   - ✅ Comparativo com mês anterior
   - ✅ Filtro por período customizado

## 🚀 Como Executar

### 1. Pré-requisitos
- PostgreSQL instalado e rodando
- .NET 8 SDK instalado
- Banco de dados criado: `financeapp`

### 2. Configurar Connection String

Verifique se o arquivo `appsettings.json` tem a connection string correta:

```json
"ConnectionStrings": {
  "DefaultConnection": "Host=localhost;Database=financeapp;Username=postgres;Password=postgres"
}
```

### 3. Criar Migration e Atualizar Banco

Execute os comandos na raiz do projeto:

```bash
# Criar a migration inicial
dotnet ef migrations add InitialCreate --project FinanceApp.Infrastructure --startup-project FinanceApp.API

# Aplicar a migration no banco
dotnet ef database update --project FinanceApp.Infrastructure --startup-project FinanceApp.API
```

### 4. Executar a API

```bash
cd FinanceApp.API
dotnet run
```

A API estará disponível em:
- HTTP: `http://localhost:5000`
- HTTPS: `https://localhost:5001`
- Swagger: `https://localhost:5001/swagger`

## 🧪 Como Testar no Swagger

### 1. Registrar Usuário

**POST** `/api/auth/register`

```json
{
  "name": "João Silva",
  "email": "joao@example.com",
  "password": "senha123"
}
```

✅ Isso criará:
- 1 usuário com senha hasheada
- 13 categorias padrão (9 de despesa + 4 de receita)

### 2. Fazer Login

**POST** `/api/auth/login`

```json
{
  "email": "joao@example.com",
  "password": "senha123"
}
```

Você receberá um token JWT. **COPIE O TOKEN!**

### 3. Autenticar no Swagger

1. Clique no botão **"Authorize"** no topo do Swagger
2. Cole o token no formato: `Bearer {seu_token_aqui}`
3. Clique em **Authorize**

Agora você pode testar todos os endpoints protegidos!

### 4. Criar uma Conta

**POST** `/api/account`

```json
{
  "name": "Conta Corrente",
  "type": 1,
  "initialBalance": 1000.00,
  "color": "#4ECDC4",
  "icon": "💰"
}
```

**Tipos de Conta:**
- 1: CheckingAccount (Conta Corrente)
- 2: SavingsAccount (Poupança)
- 3: Wallet (Carteira)
- 4: Investment (Investimento)
- 5: CreditCard (Cartão de Crédito)

### 5. Listar Categorias

**GET** `/api/category`

Você verá as 13 categorias padrão criadas automaticamente.

### 6. Criar uma Transação de Despesa

**POST** `/api/transaction`

```json
{
  "accountId": "{id_da_conta_criada}",
  "categoryId": "{id_categoria_alimentacao}",
  "amount": 50.00,
  "date": "2024-01-22T10:00:00",
  "description": "Almoço no restaurante",
  "type": 2,
  "isRecurring": false,
  "notes": "Almoço de negócios"
}
```

**Tipos de Transação:**
- 1: Income (Receita)
- 2: Expense (Despesa)
- 3: Transfer (Transferência)

### 7. Criar uma Transação de Receita

**POST** `/api/transaction`

```json
{
  "accountId": "{id_da_conta}",
  "categoryId": "{id_categoria_salario}",
  "amount": 5000.00,
  "date": "2024-01-05T00:00:00",
  "description": "Salário Janeiro",
  "type": 1,
  "isRecurring": true
}
```

### 8. Criar uma Transferência entre Contas

Primeiro crie uma segunda conta, depois:

**POST** `/api/transaction`

```json
{
  "accountId": "{id_conta_origem}",
  "categoryId": "{qualquer_categoria}",
  "amount": 200.00,
  "date": "2024-01-22T10:00:00",
  "description": "Transferência para poupança",
  "type": 3,
  "isRecurring": false,
  "destinationAccountId": "{id_conta_destino}"
}
```

### 9. Criar um Orçamento

**POST** `/api/budget`

```json
{
  "categoryId": "{id_categoria_alimentacao}",
  "month": 1,
  "year": 2024,
  "limitAmount": 500.00
}
```

### 10. Ver Status dos Orçamentos

**GET** `/api/budget/status/2024/1`

Retorna quanto foi gasto vs limite de cada orçamento.

### 11. Criar uma Meta

**POST** `/api/goal`

```json
{
  "name": "Viagem para Europa",
  "description": "Economizar para viagem de férias",
  "targetAmount": 10000.00,
  "currentAmount": 2000.00,
  "startDate": "2024-01-01T00:00:00",
  "targetDate": "2024-12-31T00:00:00",
  "color": "#FF6B6B",
  "icon": "✈️"
}
```

### 12. Ver Dashboard

**GET** `/api/dashboard/summary`

Retorna:
- Total de receitas e despesas do mês
- Saldo
- Top 5 categorias de gastos
- Evolução dos últimos 6 meses
- Comparativo com mês anterior

## 📊 Endpoints Disponíveis

### Auth
- `POST /api/auth/register` - Registrar usuário
- `POST /api/auth/login` - Login
- `GET /api/auth/me` - Validar token

### Account
- `GET /api/account` - Listar todas
- `GET /api/account/active` - Listar ativas
- `GET /api/account/{id}` - Buscar por ID
- `POST /api/account` - Criar
- `PUT /api/account/{id}` - Atualizar
- `DELETE /api/account/{id}` - Deletar

### Category
- `GET /api/category` - Listar todas
- `GET /api/category/type/{type}` - Filtrar por tipo (1=Income, 2=Expense)
- `GET /api/category/{id}` - Buscar por ID
- `POST /api/category` - Criar
- `PUT /api/category/{id}` - Atualizar
- `DELETE /api/category/{id}` - Deletar

### Transaction
- `GET /api/transaction` - Listar com paginação
- `GET /api/transaction/{id}` - Buscar por ID
- `GET /api/transaction/account/{accountId}` - Por conta
- `GET /api/transaction/category/{categoryId}` - Por categoria
- `GET /api/transaction/type/{type}` - Por tipo
- `GET /api/transaction/date-range?startDate=...&endDate=...` - Por período
- `POST /api/transaction` - Criar
- `PUT /api/transaction/{id}` - Atualizar
- `DELETE /api/transaction/{id}` - Deletar

### Budget
- `GET /api/budget` - Listar todos
- `GET /api/budget/{id}` - Buscar por ID
- `GET /api/budget/month/{year}/{month}` - Por mês
- `GET /api/budget/status/{year}/{month}` - Status (gastos vs limite)
- `POST /api/budget` - Criar
- `PUT /api/budget/{id}` - Atualizar
- `DELETE /api/budget/{id}` - Deletar

### Goal
- `GET /api/goal` - Listar todas
- `GET /api/goal/active` - Listar ativas
- `GET /api/goal/completed` - Listar completadas
- `GET /api/goal/{id}` - Buscar por ID
- `POST /api/goal` - Criar
- `PUT /api/goal/{id}` - Atualizar
- `DELETE /api/goal/{id}` - Deletar

### Dashboard
- `GET /api/dashboard/summary` - Resumo do mês atual
- `GET /api/dashboard/summary/{year}/{month}` - Resumo de mês específico
- `POST /api/dashboard/summary/custom` - Resumo por período customizado

## 🔒 Segurança Implementada

- ✅ JWT com expiração de 24 horas
- ✅ Hash de senha com BCrypt
- ✅ Todos os endpoints (exceto auth) requerem autenticação
- ✅ Validação de propriedade: usuário só acessa seus próprios dados
- ✅ Middleware de exception handling global
- ✅ Validações com FluentValidation

## 📝 Validações Implementadas

### Transações
- Amount > 0
- AccountId e CategoryId devem existir e pertencer ao usuário
- Para Transfer, DestinationAccountId é obrigatório e diferente de AccountId
- Tipo de categoria deve corresponder ao tipo de transação

### Contas
- Name obrigatório e máximo 100 caracteres
- InitialBalance pode ser negativo

### Orçamentos
- Não permitir duplicados (mesma categoria + mês/ano)
- LimitAmount > 0

### Metas
- TargetAmount > 0
- CurrentAmount >= 0
- TargetDate > StartDate (se informada)

## 🎯 Próximos Passos

1. ✅ Backend completo implementado
2. ⏳ Criar migration e testar no Swagger
3. ⏳ Desenvolver frontend com Next.js
4. ⏳ Implementar testes unitários
5. ⏳ Deploy em produção

## 📚 Tecnologias Utilizadas

- .NET 8 Web API
- Entity Framework Core 8
- PostgreSQL
- JWT Authentication
- BCrypt.Net para hash de senha
- AutoMapper
- FluentValidation
- Serilog
- Swagger/OpenAPI

## 🐛 Troubleshooting

### Erro de Connection String
Verifique se o PostgreSQL está rodando e as credenciais estão corretas.

### Erro de Migration
Execute os comandos de migration na ordem correta (veja seção "Como Executar").

### Token JWT Inválido
Certifique-se de copiar o token completo e usar o formato `Bearer {token}` no Swagger.

### Erro 401 Unauthorized
Você precisa fazer login e adicionar o token no Swagger antes de testar endpoints protegidos.

## 📞 Suporte

O sistema está pronto para uso! Qualquer dúvida, consulte este guia.

---

**Desenvolvido com ❤️ para gestão financeira pessoal**
