using BankingKata.Domain.Entities;

namespace BankingKata.Tests;

public class SavingsAccountTests
{
    [Fact]
    public void Constructor_WithValidData_ShouldCreateAccount()
    {
        var account = new SavingsAccount("SAV001", 22950, 1000);
        
        Assert.Equal("SAV001", account.AccountNumber);
        Assert.Equal(1000, account.Balance);
        Assert.Equal(22950, account.DepositCeiling);
    }

    [Fact]
    public void Constructor_WithNoInitialBalance_ShouldHaveZeroBalance()
    {
        var account = new SavingsAccount("SAV001", 22950);
        
        Assert.Equal(0, account.Balance);
    }

    [Fact]
    public void Constructor_WithInitialBalanceExceedingCeiling_ShouldThrow()
    {
        var exception = Assert.Throws<ArgumentException>(() => 
            new SavingsAccount("SAV001", 22950, 23000));
        Assert.Contains("cannot exceed deposit ceiling", exception.Message);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Constructor_WithInvalidAccountNumber_ShouldThrow(string accountNumber)
    {
        var exception = Assert.Throws<ArgumentException>(() => 
            new SavingsAccount(accountNumber!, 22950));
        Assert.Contains("Account number", exception.Message);
    }

    [Fact]
    public void Constructor_WithZeroCeiling_ShouldThrow()
    {
        var exception = Assert.Throws<ArgumentException>(() => 
            new SavingsAccount("SAV001", 0));
        Assert.Contains("Deposit ceiling must be positive", exception.Message);
    }

    [Fact]
    public void Constructor_WithNegativeCeiling_ShouldThrow()
    {
        var exception = Assert.Throws<ArgumentException>(() => 
            new SavingsAccount("SAV001", -100));
        Assert.Contains("Deposit ceiling must be positive", exception.Message);
    }

    [Fact]
    public void Deposit_WithValidAmount_ShouldIncreaseBalance()
    {
        var account = new SavingsAccount("SAV001", 22950, 1000);
        
        account.Deposit(500);
        
        Assert.Equal(1500, account.Balance);
    }

    [Fact]
    public void Deposit_UpToCeiling_ShouldSucceed()
    {
        var account = new SavingsAccount("SAV001", 22950, 22900);
        
        account.Deposit(50);
        
        Assert.Equal(22950, account.Balance);
    }

    [Fact]
    public void Deposit_ExceedingCeiling_ShouldThrow()
    {
        var account = new SavingsAccount("SAV001", 22950, 22000);
        
        var exception = Assert.Throws<InvalidOperationException>(() => 
            account.Deposit(1000));
        Assert.Contains("exceed the ceiling", exception.Message);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-10)]
    [InlineData(-1000)]
    public void Deposit_WithNonPositiveAmount_ShouldThrow(decimal amount)
    {
        var account = new SavingsAccount("SAV001", 22950);
        
        Assert.Throws<ArgumentException>(() => account.Deposit(amount));
    }

    [Fact]
    public void Withdraw_WithValidAmount_ShouldDecreaseBalance()
    {
        var account = new SavingsAccount("SAV001", 22950, 1000);
        
        account.Withdraw(300);
        
        Assert.Equal(700, account.Balance);
    }

    [Fact]
    public void Withdraw_WithExactBalance_ShouldLeaveZeroBalance()
    {
        var account = new SavingsAccount("SAV001", 22950, 1000);
        
        account.Withdraw(1000);
        
        Assert.Equal(0, account.Balance);
    }

    [Fact]
    public void Withdraw_WithAmountExceedingBalance_ShouldThrow()
    {
        var account = new SavingsAccount("SAV001", 22950, 1000);
        
        var exception = Assert.Throws<InvalidOperationException>(() => 
            account.Withdraw(1500));
        Assert.Contains("Insufficient funds", exception.Message);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-10)]
    [InlineData(-1000)]
    public void Withdraw_WithNonPositiveAmount_ShouldThrow(decimal amount)
    {
        var account = new SavingsAccount("SAV001", 22950, 1000);
        
        Assert.Throws<ArgumentException>(() => account.Withdraw(amount));
    }

    [Fact]
    public void Withdraw_WithZeroBalance_ShouldThrow()
    {
        var account = new SavingsAccount("SAV001", 22950, 0);
        
        var exception = Assert.Throws<InvalidOperationException>(() => 
            account.Withdraw(1));
        Assert.Contains("Insufficient funds", exception.Message);
    }
}
