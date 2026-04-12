using BankingKata.Application.DTOs;
using BankingKata.Application.UseCases;
using BankingKata.Infrastructure.Persistence;

namespace BankingKata.Tests;

public class SavingsAccountServiceTests
{
    private readonly SavingsAccountService _service;
    private readonly InMemorySavingsAccountRepository _repository;
    private readonly InMemoryTransactionRepository _transactionRepository;

    public SavingsAccountServiceTests()
    {
        _repository = new InMemorySavingsAccountRepository();
        _transactionRepository = new InMemoryTransactionRepository();
        _service = new SavingsAccountService(_repository, _transactionRepository);
    }

    [Fact]
    public void CreateAccount_WithValidData_ShouldCreateAccount()
    {
        var dto = new CreateSavingsAccountDto("SAV001", 22950, 1000);

        var result = _service.CreateAccount(dto);

        Assert.Equal("SAV001", result.AccountNumber);
        Assert.Equal(1000, result.Balance);
        Assert.Equal(22950, result.DepositCeiling);
    }

    [Fact]
    public void CreateAccount_WithDuplicateNumber_ShouldThrow()
    {
        _service.CreateAccount(new CreateSavingsAccountDto("SAV001", 22950));

        var ex = Assert.Throws<InvalidOperationException>(() => 
            _service.CreateAccount(new CreateSavingsAccountDto("SAV001", 22950)));
        Assert.Contains("already exists", ex.Message);
    }

    [Fact]
    public void CreateAccount_WithInitialBalanceExceedingCeiling_ShouldThrow()
    {
        var ex = Assert.Throws<ArgumentException>(() => 
            _service.CreateAccount(new CreateSavingsAccountDto("SAV001", 22950, 23000)));
        Assert.Contains("cannot exceed deposit ceiling", ex.Message);
    }

    [Fact]
    public void Deposit_ShouldIncreaseBalance()
    {
        _service.CreateAccount(new CreateSavingsAccountDto("SAV001", 22950, 1000));

        var result = _service.Deposit("SAV001", 500);

        Assert.Equal(1500, result.Balance);
    }

    [Fact]
    public void Deposit_ExceedingCeiling_ShouldThrow()
    {
        _service.CreateAccount(new CreateSavingsAccountDto("SAV001", 22950, 22000));

        var ex = Assert.Throws<InvalidOperationException>(() => 
            _service.Deposit("SAV001", 1000));
        Assert.Contains("exceed the ceiling", ex.Message);
    }

    [Fact]
    public void Deposit_WithNonExistentAccount_ShouldThrow()
    {
        var ex = Assert.Throws<InvalidOperationException>(() => 
            _service.Deposit("NONEXISTENT", 50));
        Assert.Contains("not found", ex.Message);
    }

    [Fact]
    public void Withdraw_WithValidAmount_ShouldDecreaseBalance()
    {
        _service.CreateAccount(new CreateSavingsAccountDto("SAV001", 22950, 1000));

        var result = _service.Withdraw("SAV001", 300);

        Assert.Equal(700, result.Balance);
    }

    [Fact]
    public void Withdraw_WithInsufficientFunds_ShouldThrow()
    {
        _service.CreateAccount(new CreateSavingsAccountDto("SAV001", 22950, 500));

        var ex = Assert.Throws<InvalidOperationException>(() => 
            _service.Withdraw("SAV001", 1000));
        Assert.Contains("Insufficient funds", ex.Message);
    }

    [Fact]
    public void GetAccount_WithExistingAccount_ShouldReturnAccount()
    {
        _service.CreateAccount(new CreateSavingsAccountDto("SAV001", 22950, 1000));

        var result = _service.GetAccount("SAV001");

        Assert.NotNull(result);
        Assert.Equal("SAV001", result.AccountNumber);
    }

    [Fact]
    public void GetAccount_WithNonExistentAccount_ShouldReturnNull()
    {
        var result = _service.GetAccount("NONEXISTENT");

        Assert.Null(result);
    }

    [Fact]
    public void GetAllAccounts_ShouldReturnAllCreatedAccounts()
    {
        _service.CreateAccount(new CreateSavingsAccountDto("SAV001", 22950));
        _service.CreateAccount(new CreateSavingsAccountDto("SAV002", 10000));
        _service.CreateAccount(new CreateSavingsAccountDto("SAV003", 5000));

        var result = _service.GetAllAccounts().ToList();

        Assert.Equal(3, result.Count);
    }

    [Fact]
    public void GetStatement_WithRecentTransactions_ShouldReturnOperations()
    {
        _service.CreateAccount(new CreateSavingsAccountDto("SAV001", 22950, 1000));
        _service.Deposit("SAV001", 500);
        _service.Withdraw("SAV001", 300);

        var statement = _service.GetStatement("SAV001");

        Assert.Equal("SAV001", statement.AccountNumber);
        Assert.Equal("Livret", statement.AccountType);
        Assert.Equal(1200, statement.CurrentBalance);
        Assert.Equal(2, statement.Operations.Count());
    }

    [Fact]
    public void GetStatement_WithNonExistentAccount_ShouldThrow()
    {
        var ex = Assert.Throws<InvalidOperationException>(() => 
            _service.GetStatement("NONEXISTENT"));
        Assert.Contains("not found", ex.Message);
    }
}
