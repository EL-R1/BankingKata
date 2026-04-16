namespace BankingKata.Domain.Entities;

public class Transaction
{
    public Guid Id { get; }
    public string AccountNumber { get; }
    public decimal Amount { get; }
    public TransactionType Type { get; }
    public DateTime Date { get; }
    public decimal BalanceAfterTransaction { get; }

    public Transaction(string accountNumber, decimal amount, TransactionType type, decimal balanceAfterTransaction, DateTime? date = null)
    {
        if (string.IsNullOrWhiteSpace(accountNumber))
            throw new ArgumentException("Account number cannot be empty", nameof(accountNumber));
        if (amount <= 0)
            throw new ArgumentException("Amount must be positive", nameof(amount));
        
        Id = Guid.NewGuid();
        AccountNumber = accountNumber;
        Amount = amount;
        Type = type;
        Date = date ?? DateTime.UtcNow;
        BalanceAfterTransaction = balanceAfterTransaction;
    }

    public static Transaction CreateDeposit(string accountNumber, decimal amount, decimal balanceAfterTransaction, DateTime? date = null)
        => new(accountNumber, amount, TransactionType.Deposit, balanceAfterTransaction, date);

    public static Transaction CreateWithdrawal(string accountNumber, decimal amount, decimal balanceAfterTransaction, DateTime? date = null)
        => new(accountNumber, amount, TransactionType.Withdrawal, balanceAfterTransaction, date);
}

public enum TransactionType
{
    Deposit,
    Withdrawal
}
