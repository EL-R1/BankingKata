using BankingKata.Application.DTOs;
using BankingKata.Application.UseCases;
using BankingKata.Infrastructure.Persistence;

namespace BankingKata.Tests;

public class BankAccountServiceTests
{
    private readonly BankAccountService _service;
    private readonly InMemoryBankAccountRepository _repository;
    private readonly InMemoryTransactionRepository _transactionRepository;

    public BankAccountServiceTests()
    {
        _repository = new InMemoryBankAccountRepository();
        _transactionRepository = new InMemoryTransactionRepository();
        _service = new BankAccountService(_repository, _transactionRepository);
    }

    [Fact]
    public void CreateAccount_WithValidData_ShouldCreateAccount()
    {
        var dto = new CreateAccountDto("ACC001", 100);

        var result = _service.CreateAccount(dto);

        Assert.Equal("ACC001", result.AccountNumber);
        Assert.Equal(100, result.Balance);
    }

    [Fact]
    public void CreateAccount_WithDuplicateNumber_ShouldThrow()
    {
        _service.CreateAccount(new CreateAccountDto("ACC001"));

        var ex = Assert.Throws<InvalidOperationException>(() => 
            _service.CreateAccount(new CreateAccountDto("ACC001")));
        Assert.Contains("already exists", ex.Message);
    }

    [Fact]
    public void Deposit_ShouldIncreaseBalance()
    {
        _service.CreateAccount(new CreateAccountDto("ACC001", 100));

        var result = _service.Deposit("ACC001", 50);

        Assert.Equal(150, result.Balance);
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
        _service.CreateAccount(new CreateAccountDto("ACC001", 100));

        var result = _service.Withdraw("ACC001", 30);

        Assert.Equal(70, result.Balance);
    }

    [Fact]
    public void Withdraw_WithInsufficientFunds_ShouldThrow()
    {
        _service.CreateAccount(new CreateAccountDto("ACC001", 50));

        var ex = Assert.Throws<InvalidOperationException>(() => 
            _service.Withdraw("ACC001", 100));
        Assert.Contains("Insufficient funds", ex.Message);
    }

    [Fact]
    public void GetAccount_WithExistingAccount_ShouldReturnAccount()
    {
        _service.CreateAccount(new CreateAccountDto("ACC001", 100));

        var result = _service.GetAccount("ACC001");

        Assert.NotNull(result);
        Assert.Equal("ACC001", result.AccountNumber);
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
        _service.CreateAccount(new CreateAccountDto("ACC001"));
        _service.CreateAccount(new CreateAccountDto("ACC002"));
        _service.CreateAccount(new CreateAccountDto("ACC003"));

        var result = _service.GetAllAccounts().ToList();

        Assert.Equal(3, result.Count);
    }

    [Fact]
    public void CreateAccount_WithOverdraft_ShouldSetOverdraft()
    {
        var dto = new CreateAccountDto("ACC001", 100, 500);

        var result = _service.CreateAccount(dto);

        Assert.Equal(500, result.OverdraftLimit);
    }

    [Fact]
    public void Withdraw_WithOverdraft_WithinLimit_ShouldSucceed()
    {
        _service.CreateAccount(new CreateAccountDto("ACC001", 100, 200));

        var result = _service.Withdraw("ACC001", 250);

        Assert.Equal(-150, result.Balance);
    }

    [Fact]
    public void Withdraw_WithOverdraft_ExceedingLimit_ShouldThrow()
    {
        _service.CreateAccount(new CreateAccountDto("ACC001", 100, 200));

        var ex = Assert.Throws<InvalidOperationException>(() => 
            _service.Withdraw("ACC001", 350));
        Assert.Contains("Insufficient funds", ex.Message);
    }

    [Fact]
    public void SetOverdraftLimit_ShouldUpdateOverdraft()
    {
        _service.CreateAccount(new CreateAccountDto("ACC001", 100, 0));

        var result = _service.SetOverdraftLimit("ACC001", 500);

        Assert.Equal(500, result.OverdraftLimit);
    }

    [Fact]
    public void SetOverdraftLimit_WithNonExistentAccount_ShouldThrow()
    {
        var ex = Assert.Throws<InvalidOperationException>(() => 
            _service.SetOverdraftLimit("NONEXISTENT", 500));
        Assert.Contains("not found", ex.Message);
    }

    [Fact]
    public void SetOverdraftLimit_WithNegativeAmount_ShouldThrow()
    {
        _service.CreateAccount(new CreateAccountDto("ACC001", 100));

        var ex = Assert.Throws<ArgumentException>(() => 
            _service.SetOverdraftLimit("ACC001", -100));
        Assert.Contains("cannot be negative", ex.Message);
    }

    [Fact]
    public void GetStatement_WithRecentTransactions_ShouldReturnOperations()
    {
        _service.CreateAccount(new CreateAccountDto("ACC001", 100));
        _service.Deposit("ACC001", 50);
        _service.Withdraw("ACC001", 30);

        var statement = _service.GetStatement("ACC001");

        Assert.Equal("ACC001", statement.AccountNumber);
        Assert.Equal("Compte Courant", statement.AccountType);
        Assert.Equal(120, statement.CurrentBalance);
        Assert.Equal(2, statement.Operations.Count());
    }

    [Fact]
    public void GetStatement_WithNonExistentAccount_ShouldThrow()
    {
        var ex = Assert.Throws<InvalidOperationException>(() => 
            _service.GetStatement("NONEXISTENT"));
        Assert.Contains("not found", ex.Message);
    }

    [Fact]
    public void GetStatement_ShouldSortOperationsByDateDescending()
    {
        _service.CreateAccount(new CreateAccountDto("ACC001", 100));
        _service.Deposit("ACC001", 50);
        _service.Withdraw("ACC001", 30);
        _service.Deposit("ACC001", 20);

        var statement = _service.GetStatement("ACC001");
        var operations = statement.Operations.ToList();

        Assert.Equal(3, operations.Count);
        Assert.Equal("Deposit", operations[0].Type);
        Assert.Equal("Withdrawal", operations[1].Type);
        Assert.Equal("Deposit", operations[2].Type);
    }

    [Fact]
    public void GetStatementInRange_ShouldFilterByDate()
    {
        _service.CreateAccount(new CreateAccountDto("ACC001", 100));
        _service.Deposit("ACC001", 50);

        var fromDate = DateTime.UtcNow.AddDays(-7);
        var toDate = DateTime.UtcNow;

        var statement = _service.GetStatementInRange("ACC001", fromDate, toDate);

        Assert.Single(statement.Operations);
    }
}
