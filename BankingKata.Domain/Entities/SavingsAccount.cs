namespace BankingKata.Domain.Entities;

public class SavingsAccount
{
    public string AccountNumber { get; }
    public decimal Balance { get; private set; }
    public decimal DepositCeiling { get; }

    public SavingsAccount(string accountNumber, decimal depositCeiling, decimal initialBalance = 0)
    {
        if (string.IsNullOrWhiteSpace(accountNumber))
            throw new ArgumentException("Account number cannot be empty", nameof(accountNumber));
        if (depositCeiling <= 0)
            throw new ArgumentException("Deposit ceiling must be positive", nameof(depositCeiling));
        if (initialBalance > depositCeiling)
            throw new ArgumentException("Initial balance cannot exceed deposit ceiling", nameof(initialBalance));
        
        AccountNumber = accountNumber;
        DepositCeiling = depositCeiling;
        Balance = initialBalance;
    }

    public void Deposit(decimal amount)
    {
        if (amount <= 0)
            throw new ArgumentException("Deposit amount must be positive", nameof(amount));
        
        if (Balance + amount > DepositCeiling)
            throw new InvalidOperationException($"Deposit would exceed the ceiling of {DepositCeiling}");
        
        Balance += amount;
    }

    public void Withdraw(decimal amount)
    {
        if (amount <= 0)
            throw new ArgumentException("Withdrawal amount must be positive", nameof(amount));
        
        if (amount > Balance)
            throw new InvalidOperationException("Insufficient funds for withdrawal");
        
        Balance -= amount;
    }
}
