namespace FinanceApp.Domain.Enums;

public enum AccountType
{
    CheckingAccount = 1,
    SavingsAccount = 2,
    Wallet = 3,
    Investment = 4,
    CreditCard = 5
}

public enum TransactionType
{
    Income = 1,
    Expense = 2,
    Transfer = 3,
    GoalDeposit = 4,
    GoalWithdraw = 5
}

public enum CategoryType
{
    Income = 1,
    Expense = 2
}

public enum GroupRole
{
    Owner = 1,
    Member = 2
}

public enum ViewContext
{
    Own = 1,
    Member = 2,
    All = 3
}
