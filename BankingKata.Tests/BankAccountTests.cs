using BankingKata.Domain.Entities;

namespace BankingKata.Tests;

public class BankAccountTests
{
    [Fact]
    public void Constructor_WithValidAccountNumber_ShouldCreateAccount()
    {
        var account = new BankAccount("FR7612345678901234567890123", 100);
        
        Assert.Equal("FR7612345678901234567890123", account.AccountNumber);
        Assert.Equal(100, account.Balance);
    }

    [Fact]
    public void Constructor_WithNoInitialBalance_ShouldHaveZeroBalance()
    {
        var account = new BankAccount("ACC001");
        
        Assert.Equal(0, account.Balance);
    }

    [Fact]
    public void Constructor_WithOverdraftLimit_ShouldSetOverdraft()
    {
        var account = new BankAccount("ACC001", 100, 500);
        
        Assert.Equal(500, account.OverdraftLimit);
    }

    [Fact]
    public void Constructor_WithNegativeOverdraft_ShouldThrow()
    {
        var exception = Assert.Throws<ArgumentException>(() => 
            new BankAccount("ACC001", 0, -100));
        Assert.Contains("Overdraft limit cannot be negative", exception.Message);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Constructor_WithInvalidAccountNumber_ShouldThrow(string accountNumber)
    {
        var exception = Assert.Throws<ArgumentException>(() => new BankAccount(accountNumber!));
        Assert.Contains("Account number", exception.Message);
    }

    [Fact]
    public void Deposit_WithPositiveAmount_ShouldIncreaseBalance()
    {
        var account = new BankAccount("ACC001", 100);
        
        account.Deposit(50);
        
        Assert.Equal(150, account.Balance);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-10)]
    [InlineData(-1000)]
    public void Deposit_WithNonPositiveAmount_ShouldThrow(decimal amount)
    {
        var account = new BankAccount("ACC001");
        
        Assert.Throws<ArgumentException>(() => account.Deposit(amount));
    }

    [Fact]
    public void Withdraw_WithValidAmount_ShouldDecreaseBalance()
    {
        var account = new BankAccount("ACC001", 100);
        
        account.Withdraw(30);
        
        Assert.Equal(70, account.Balance);
    }

    [Fact]
    public void Withdraw_WithExactBalance_ShouldLeaveZeroBalance()
    {
        var account = new BankAccount("ACC001", 100);
        
        account.Withdraw(100);
        
        Assert.Equal(0, account.Balance);
    }

    [Fact]
    public void Withdraw_WithAmountExceedingBalance_ShouldThrow()
    {
        var account = new BankAccount("ACC001", 100);
        
        var exception = Assert.Throws<InvalidOperationException>(() => account.Withdraw(150));
        Assert.Contains("Insufficient funds", exception.Message);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-10)]
    [InlineData(-1000)]
    public void Withdraw_WithNonPositiveAmount_ShouldThrow(decimal amount)
    {
        var account = new BankAccount("ACC001", 100);
        
        Assert.Throws<ArgumentException>(() => account.Withdraw(amount));
    }

    [Fact]
    public void Withdraw_WithOverdraft_WithinLimit_ShouldSucceed()
    {
        var account = new BankAccount("ACC001", 100, 200);
        
        account.Withdraw(250);
        
        Assert.Equal(-150, account.Balance);
    }

    [Fact]
    public void Withdraw_WithOverdraft_ExactlyAtLimit_ShouldSucceed()
    {
        var account = new BankAccount("ACC001", 100, 200);
        
        account.Withdraw(300);
        
        Assert.Equal(-200, account.Balance);
    }

    [Fact]
    public void Withdraw_WithOverdraft_ExceedingLimit_ShouldThrow()
    {
        var account = new BankAccount("ACC001", 100, 200);
        
        var exception = Assert.Throws<InvalidOperationException>(() => account.Withdraw(350));
        Assert.Contains("Insufficient funds", exception.Message);
    }

    [Fact]
    public void Withdraw_WithOverdraft_ExactlyAtZero_ShouldSucceed()
    {
        var account = new BankAccount("ACC001", 0, 100);
        
        account.Withdraw(100);
        
        Assert.Equal(-100, account.Balance);
    }

    [Fact]
    public void SetOverdraftLimit_WithValidAmount_ShouldUpdate()
    {
        var account = new BankAccount("ACC001", 100);
        
        account.SetOverdraftLimit(500);
        
        Assert.Equal(500, account.OverdraftLimit);
    }

    [Fact]
    public void SetOverdraftLimit_WithNegativeAmount_ShouldThrow()
    {
        var account = new BankAccount("ACC001", 100);
        
        var exception = Assert.Throws<ArgumentException>(() => account.SetOverdraftLimit(-50));
        Assert.Contains("Overdraft limit cannot be negative", exception.Message);
    }

    [Fact]
    public void Withdraw_AfterSettingOverdraft_ShouldUseNewLimit()
    {
        var account = new BankAccount("ACC001", 100, 0);
        
        account.SetOverdraftLimit(200);
        account.Withdraw(250);
        
        Assert.Equal(-150, account.Balance);
    }
}
