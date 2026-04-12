using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Json;

namespace BankingKata.Api.Tests;

public class SavingsControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public SavingsControllerTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task CreateAccount_ShouldReturnCreated()
    {
        var response = await _client.PostAsJsonAsync("/api/savings", 
            new { accountNumber = "SAV001", depositCeiling = 22950, initialBalance = 1000 });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task CreateAccount_Duplicate_ShouldReturnConflict()
    {
        await _client.PostAsJsonAsync("/api/savings", 
            new { accountNumber = "SAV002", depositCeiling = 22950 });

        var response = await _client.PostAsJsonAsync("/api/savings", 
            new { accountNumber = "SAV002", depositCeiling = 22950 });

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task CreateAccount_WithInitialBalanceExceedingCeiling_ShouldReturnBadRequest()
    {
        var response = await _client.PostAsJsonAsync("/api/savings", 
            new { accountNumber = "SAV003", depositCeiling = 22950, initialBalance = 23000 });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetAccount_Existing_ShouldReturnAccount()
    {
        await _client.PostAsJsonAsync("/api/savings", 
            new { accountNumber = "SAV004", depositCeiling = 22950, initialBalance = 1000 });

        var response = await _client.GetAsync("/api/savings/SAV004");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var account = await response.Content.ReadFromJsonAsync<SavingsAccountResponse>();
        Assert.Equal("SAV004", account!.accountNumber);
        Assert.Equal(1000, account.balance);
        Assert.Equal(22950, account.depositCeiling);
    }

    [Fact]
    public async Task GetAccount_NonExisting_ShouldReturnNotFound()
    {
        var response = await _client.GetAsync("/api/savings/NONEXISTENT");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Deposit_ShouldIncreaseBalance()
    {
        await _client.PostAsJsonAsync("/api/savings", 
            new { accountNumber = "SAV005", depositCeiling = 22950, initialBalance = 1000 });

        var response = await _client.PostAsJsonAsync("/api/savings/SAV005/deposit", 
            new { amount = 500 });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var account = await response.Content.ReadFromJsonAsync<SavingsAccountResponse>();
        Assert.Equal(1500, account!.balance);
    }

    [Fact]
    public async Task Deposit_ExceedingCeiling_ShouldReturnBadRequest()
    {
        await _client.PostAsJsonAsync("/api/savings", 
            new { accountNumber = "SAV006", depositCeiling = 22950, initialBalance = 22000 });

        var response = await _client.PostAsJsonAsync("/api/savings/SAV006/deposit", 
            new { amount = 1000 });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Withdraw_WithSufficientFunds_ShouldDecreaseBalance()
    {
        await _client.PostAsJsonAsync("/api/savings", 
            new { accountNumber = "SAV007", depositCeiling = 22950, initialBalance = 1000 });

        var response = await _client.PostAsJsonAsync("/api/savings/SAV007/withdraw", 
            new { amount = 300 });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var account = await response.Content.ReadFromJsonAsync<SavingsAccountResponse>();
        Assert.Equal(700, account!.balance);
    }

    [Fact]
    public async Task Withdraw_WithInsufficientFunds_ShouldReturnBadRequest()
    {
        await _client.PostAsJsonAsync("/api/savings", 
            new { accountNumber = "SAV008", depositCeiling = 22950, initialBalance = 500 });

        var response = await _client.PostAsJsonAsync("/api/savings/SAV008/withdraw", 
            new { amount = 1000 });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetAllAccounts_ShouldReturnAllAccounts()
    {
        await _client.PostAsJsonAsync("/api/savings", 
            new { accountNumber = "SAV009", depositCeiling = 22950 });
        await _client.PostAsJsonAsync("/api/savings", 
            new { accountNumber = "SAV010", depositCeiling = 10000 });

        var response = await _client.GetAsync("/api/savings");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var accounts = await response.Content.ReadFromJsonAsync<List<SavingsAccountResponse>>();
        Assert.True(accounts!.Count >= 2);
    }

    [Fact]
    public async Task GetStatement_ShouldReturnStatement()
    {
        await _client.PostAsJsonAsync("/api/savings", 
            new { accountNumber = "SAV020", depositCeiling = 22950, initialBalance = 1000 });
        await _client.PostAsJsonAsync("/api/savings/SAV020/deposit", 
            new { amount = 500 });
        await _client.PostAsJsonAsync("/api/savings/SAV020/withdraw", 
            new { amount = 200 });

        var response = await _client.GetAsync("/api/savings/SAV020/statement");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var statement = await response.Content.ReadFromJsonAsync<StatementResponse>();
        Assert.Equal("SAV020", statement!.accountNumber);
        Assert.Equal("Livret", statement.accountType);
        Assert.Equal(1300, statement.currentBalance);
        Assert.Equal(2, statement.operations.Count);
    }

    [Fact]
    public async Task GetStatement_NonExistingAccount_ShouldReturnNotFound()
    {
        var response = await _client.GetAsync("/api/savings/NONEXISTENT/statement");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    private record SavingsAccountResponse(string accountNumber, decimal balance, decimal depositCeiling);

    private record StatementResponse(
        string accountNumber, 
        string accountType, 
        decimal currentBalance, 
        DateTime statementDate,
        List<OperationResponse> operations
    );

    private record OperationResponse(Guid id, string accountNumber, decimal amount, string type, DateTime date, decimal balanceAfterTransaction);
}
