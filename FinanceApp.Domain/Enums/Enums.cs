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
    Transfer = 3
}

public enum CategoryType
{
    Income = 1,
    Expense = 2
}
