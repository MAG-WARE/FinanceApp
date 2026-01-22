# 🧪 Guia de Testes - FinanceApp Backend

## 📋 Pré-requisitos

1. ✅ .NET 8 SDK instalado
2. ✅ PostgreSQL rodando
3. ✅ Banco de dados criado: `financeapp`

---

## 🚀 Passo 1: Compilar e Restaurar Pacotes

```bash
# Na raiz do projeto FinanceApp
dotnet restore
dotnet build
```

Se houver erros, execute:
```bash
dotnet clean
dotnet restore
dotnet build
```

---

## 🗄️ Passo 2: Criar e Aplicar Migration

```bash
# Criar a migration inicial (apenas uma vez)
dotnet ef migrations add InitialCreate --project FinanceApp.Infrastructure --startup-project FinanceApp.API

# Aplicar a migration ao banco de dados
dotnet ef database update --project FinanceApp.Infrastructure --startup-project FinanceApp.API
```

**Nota**: Se não tiver o `dotnet-ef` instalado:
```bash
dotnet tool install --global dotnet-ef
```

---

## ▶️ Passo 3: Executar a API

```bash
cd FinanceApp.API
dotnet run
```

A API estará disponível em:
- **HTTP**: http://localhost:5000
- **HTTPS**: https://localhost:5001
- **Swagger**: https://localhost:5001/swagger

---

## 🧪 Passo 4: Testar no Swagger

Acesse: **https://localhost:5001/swagger**

### 📝 Teste 1: Registrar Usuário

**Endpoint**: `POST /api/auth/register`

Clique em "Try it out" e use este JSON:

```json
{
  "name": "João Silva",
  "email": "joao@teste.com",
  "password": "senha123"
}
```

**Resultado Esperado**:
- Status: 200 OK
- Retorna: Token JWT + dados do usuário
- **13 categorias padrão criadas automaticamente**

**Copie o token retornado!** Exemplo:
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "user": {
    "id": "guid-aqui",
    "name": "João Silva",
    "email": "joao@teste.com"
  }
}
```

---

### 🔐 Teste 2: Autenticar no Swagger

1. Clique no botão **"Authorize"** (cadeado verde no topo)
2. Digite: `Bearer SEU_TOKEN_AQUI` (sem as chaves)
3. Clique em **"Authorize"**
4. Feche o modal

Agora você está autenticado! ✅

---

### 💰 Teste 3: Criar uma Conta

**Endpoint**: `POST /api/account`

```json
{
  "name": "Conta Corrente Nubank",
  "type": 1,
  "initialBalance": 1500.00,
  "color": "#8B10AE",
  "icon": "💳"
}
```

**Tipos de Conta**:
- `1` = Conta Corrente
- `2` = Poupança
- `3` = Carteira
- `4` = Investimento
- `5` = Cartão de Crédito

**Resultado Esperado**:
```json
{
  "id": "guid-da-conta",
  "name": "Conta Corrente Nubank",
  "type": 1,
  "initialBalance": 1500.00,
  "currentBalance": 1500.00,
  "isActive": true,
  "color": "#8B10AE",
  "icon": "💳"
}
```

---

### 📂 Teste 4: Listar Categorias

**Endpoint**: `GET /api/category`

Clique em "Execute" sem parâmetros.

**Resultado Esperado**: Lista com 13 categorias:

**Despesas (9)**:
- 🍔 Alimentação
- 🚗 Transporte
- 🏠 Moradia
- 💊 Saúde
- 📚 Educação
- 🎮 Lazer
- 🛒 Compras
- 📄 Contas
- 📦 Outros

**Receitas (4)**:
- 💰 Salário
- 💼 Freelance
- 📈 Investimentos
- 💵 Outras Receitas

**Copie o ID de 2 categorias**: uma de receita (Salário) e uma de despesa (Alimentação)

---

### 💸 Teste 5: Criar Transação de Receita (Salário)

**Endpoint**: `POST /api/transaction`

```json
{
  "accountId": "COLE_ID_DA_CONTA_AQUI",
  "categoryId": "COLE_ID_CATEGORIA_SALARIO",
  "amount": 5000.00,
  "date": "2024-01-05T00:00:00",
  "description": "Salário Janeiro 2024",
  "type": 1,
  "isRecurring": true,
  "notes": "Pagamento mensal"
}
```

**Tipos de Transação**:
- `1` = Receita (Income)
- `2` = Despesa (Expense)
- `3` = Transferência (Transfer)

---

### 🍔 Teste 6: Criar Transação de Despesa (Alimentação)

**Endpoint**: `POST /api/transaction`

```json
{
  "accountId": "COLE_ID_DA_CONTA_AQUI",
  "categoryId": "COLE_ID_CATEGORIA_ALIMENTACAO",
  "amount": 85.50,
  "date": "2024-01-22T12:30:00",
  "description": "Almoço no restaurante",
  "type": 2,
  "isRecurring": false,
  "notes": "Almoço de negócios"
}
```

---

### 💳 Teste 7: Verificar Saldo Atualizado

**Endpoint**: `GET /api/account/{id}`

Cole o ID da conta criada.

**Resultado Esperado**:
```json
{
  "currentBalance": 6414.50
}
```

**Cálculo**: 1500 (inicial) + 5000 (salário) - 85.50 (almoço) = 6414.50 ✅

---

### 📊 Teste 8: Criar Orçamento

**Endpoint**: `POST /api/budget`

```json
{
  "categoryId": "COLE_ID_CATEGORIA_ALIMENTACAO",
  "month": 1,
  "year": 2024,
  "limitAmount": 1000.00
}
```

---

### 📈 Teste 9: Verificar Status do Orçamento

**Endpoint**: `GET /api/budget/status/2024/1`

**Resultado Esperado**:
```json
[
  {
    "id": "guid-do-orcamento",
    "categoryName": "Alimentação",
    "month": 1,
    "year": 2024,
    "limitAmount": 1000.00,
    "spentAmount": 85.50,
    "remainingAmount": 914.50,
    "percentageUsed": 8.55,
    "isExceeded": false
  }
]
```

---

### 🎯 Teste 10: Criar uma Meta Financeira

**Endpoint**: `POST /api/goal`

```json
{
  "name": "Viagem para Europa",
  "description": "Economizar para viagem de férias em julho",
  "targetAmount": 15000.00,
  "currentAmount": 2000.00,
  "startDate": "2024-01-01T00:00:00",
  "targetDate": "2024-07-01T00:00:00",
  "color": "#FF6B6B",
  "icon": "✈️"
}
```

**Resultado Esperado**:
```json
{
  "id": "guid-da-meta",
  "name": "Viagem para Europa",
  "targetAmount": 15000.00,
  "currentAmount": 2000.00,
  "progressPercentage": 13.33,
  "remainingAmount": 13000.00,
  "isCompleted": false
}
```

---

### 📊 Teste 11: Ver Dashboard

**Endpoint**: `GET /api/dashboard/summary`

**Resultado Esperado**:
```json
{
  "totalIncome": 5000.00,
  "totalExpenses": 85.50,
  "balance": 4914.50,
  "month": 1,
  "year": 2024,
  "topSpendingCategories": [
    {
      "categoryId": "guid",
      "categoryName": "Alimentação",
      "amount": 85.50,
      "percentage": 100.00,
      "color": "#FF6B6B"
    }
  ],
  "balanceHistory": [...],
  "comparison": {
    "currentMonthIncome": 5000.00,
    "currentMonthExpenses": 85.50,
    ...
  }
}
```

---

## 🔄 Teste 12: Criar Segunda Conta para Transferência

**Endpoint**: `POST /api/account`

```json
{
  "name": "Poupança",
  "type": 2,
  "initialBalance": 0,
  "color": "#00B894",
  "icon": "🏦"
}
```

**Copie o ID desta conta!**

---

### 💸 Teste 13: Fazer Transferência Entre Contas

**Endpoint**: `POST /api/transaction`

```json
{
  "accountId": "ID_CONTA_CORRENTE",
  "categoryId": "QUALQUER_CATEGORIA_ID",
  "amount": 500.00,
  "date": "2024-01-22T15:00:00",
  "description": "Transferência para poupança",
  "type": 3,
  "isRecurring": false,
  "destinationAccountId": "ID_CONTA_POUPANCA"
}
```

---

### ✅ Teste 14: Verificar Saldos Após Transferência

**Endpoint**: `GET /api/account`

**Resultado Esperado**:

**Conta Corrente**:
- Antes: 6414.50
- Depois: 5914.50 (- 500)

**Poupança**:
- Antes: 0
- Depois: 500.00 (+ 500)

---

## 🧪 Teste 15: Listar Transações com Filtros

### Por Conta
`GET /api/transaction/account/{accountId}`

### Por Categoria
`GET /api/transaction/category/{categoryId}`

### Por Tipo
`GET /api/transaction/type/1` (1=Income, 2=Expense, 3=Transfer)

### Por Período
`GET /api/transaction/date-range?startDate=2024-01-01&endDate=2024-01-31`

---

## 🔐 Teste 16: Validar Token

**Endpoint**: `GET /api/auth/me`

**Resultado Esperado**:
```json
{
  "userId": "guid-do-usuario",
  "email": "joao@teste.com",
  "name": "João Silva"
}
```

---

## ❌ Testes de Validação (Devem Falhar)

### Teste 1: Criar transação com valor negativo
```json
{
  "accountId": "id-valido",
  "categoryId": "id-valido",
  "amount": -50.00,
  "...": "..."
}
```
**Esperado**: 400 Bad Request - "Valor deve ser maior que zero"

### Teste 2: Criar orçamento duplicado
Tente criar outro orçamento para a mesma categoria/mês/ano.

**Esperado**: 400 Bad Request - "Já existe um orçamento para esta categoria e período"

### Teste 3: Transferência com mesma conta origem/destino
```json
{
  "accountId": "mesmo-id",
  "destinationAccountId": "mesmo-id",
  "type": 3,
  "...": "..."
}
```
**Esperado**: 400 Bad Request - "Conta de origem e destino não podem ser iguais"

### Teste 4: Acessar endpoint sem token
Remova a autorização (Logout no Swagger) e tente acessar qualquer endpoint.

**Esperado**: 401 Unauthorized

---

## 📝 Checklist de Testes Completos

- [ ] ✅ Registro de usuário
- [ ] ✅ Login e obtenção de token
- [ ] ✅ Autenticação no Swagger
- [ ] ✅ Criar conta
- [ ] ✅ Listar categorias padrão (13 categorias)
- [ ] ✅ Criar transação de receita
- [ ] ✅ Criar transação de despesa
- [ ] ✅ Verificar cálculo automático de saldo
- [ ] ✅ Criar orçamento
- [ ] ✅ Verificar status de orçamento
- [ ] ✅ Criar meta financeira
- [ ] ✅ Ver dashboard com resumo
- [ ] ✅ Criar segunda conta
- [ ] ✅ Fazer transferência entre contas
- [ ] ✅ Verificar saldos após transferência
- [ ] ✅ Testar filtros de transações
- [ ] ✅ Validações de erro funcionando

---

## 🐛 Troubleshooting

### Erro: "Cannot connect to database"
```bash
# Verifique se o PostgreSQL está rodando
sudo service postgresql status

# Inicie se necessário
sudo service postgresql start
```

### Erro: "Database 'financeapp' does not exist"
```bash
# Conecte ao PostgreSQL
psql -U postgres

# Crie o banco
CREATE DATABASE financeapp;
\q
```

### Erro: "The ConnectionString property has not been initialized"
Verifique o arquivo `appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=financeapp;Username=postgres;Password=postgres"
  }
}
```

### Erro 401 Unauthorized
- Certifique-se de copiar o token completo
- Use o formato: `Bearer {token}`
- Verifique se o token não expirou (válido por 24h)

---

## 🎉 Resultado Final

Se todos os testes passaram, você tem:

✅ Sistema de autenticação funcionando
✅ CRUD completo de contas, categorias, transações, orçamentos e metas
✅ Cálculo automático de saldos
✅ Transferências entre contas
✅ Orçamentos com tracking de gastos
✅ Dashboard com relatórios
✅ Validações de segurança
✅ Todas as regras de negócio implementadas

**Parabéns! O backend está 100% funcional!** 🚀

---

## 📞 Próximos Passos

1. ✅ Backend completo e testado
2. ⏳ Desenvolver frontend em Next.js
3. ⏳ Implementar testes unitários
4. ⏳ Deploy em produção

---

**Desenvolvido com ❤️ para gestão financeira pessoal**
