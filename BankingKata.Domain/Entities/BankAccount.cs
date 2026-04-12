namespace BankingKata.Domain.Entities;

public class BankAccount
{
    public string AccountNumber { get; }
    public decimal Balance { get; private set; }
    public decimal OverdraftLimit { get; private set; }

    public BankAccount(string accountNumber, decimal initialBalance = 0, decimal overdraftLimit = 0)
    {
        if (string.IsNullOrWhiteSpace(accountNumber))
            throw new ArgumentException("Account number cannot be empty", nameof(accountNumber));
        if (overdraftLimit < 0)
            throw new ArgumentException("Overdraft limit cannot be negative", nameof(overdraftLimit));
        
        AccountNumber = accountNumber;
        Balance = initialBalance;
        OverdraftLimit = overdraftLimit;
    }

    public void Deposit(decimal amount)
    {
        if (amount <= 0)
            throw new ArgumentException("Deposit amount must be positive", nameof(amount));
        
        Balance += amount;
    }

    public void Withdraw(decimal amount)
    {
        if (amount <= 0)
            throw new ArgumentException("Withdrawal amount must be positive", nameof(amount));
        
        if (amount > Balance + OverdraftLimit)
            throw new InvalidOperationException("Insufficient funds for withdrawal");
        
        Balance -= amount;
    }

    public void SetOverdraftLimit(decimal limit)
    {
        if (limit < 0)
            throw new ArgumentException("Overdraft limit cannot be negative", nameof(limit));
        
        OverdraftLimit = limit;
    }
}
