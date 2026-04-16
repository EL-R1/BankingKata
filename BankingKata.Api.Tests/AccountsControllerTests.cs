using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Json;

namespace BankingKata.Api.Tests;

public class AccountsControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public AccountsControllerTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task CreateAccount_ShouldReturnCreated()
    {
        var response = await _client.PostAsJsonAsync("/api/accounts", 
            new { accountNumber = "TEST001", initialBalance = 100 });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task CreateAccount_Duplicate_ShouldReturnConflict()
    {
        await _client.PostAsJsonAsync("/api/accounts", 
            new { accountNumber = "TEST002", initialBalance = 100 });

        var response = await _client.PostAsJsonAsync("/api/accounts", 
            new { accountNumber = "TEST002", initialBalance = 200 });

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task CreateAccount_WithOverdraft_ShouldSetOverdraft()
    {
        var response = await _client.PostAsJsonAsync("/api/accounts", 
            new { accountNumber = "TEST020", initialBalance = 100, overdraftLimit = 500 });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var account = await response.Content.ReadFromJsonAsync<AccountResponse>();
        Assert.Equal(500, account!.overdraftLimit);
    }

    [Fact]
    public async Task GetAccount_Existing_ShouldReturnAccount()
    {
        await _client.PostAsJsonAsync("/api/accounts", 
            new { accountNumber = "TEST003", initialBalance = 100 });

        var response = await _client.GetAsync("/api/accounts/TEST003");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var account = await response.Content.ReadFromJsonAsync<AccountResponse>();
        Assert.Equal("TEST003", account!.accountNumber);
        Assert.Equal(100, account.balance);
    }

    [Fact]
    public async Task GetAccount_NonExisting_ShouldReturnNotFound()
    {
        var response = await _client.GetAsync("/api/accounts/NONEXISTENT");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Deposit_ShouldIncreaseBalance()
    {
        await _client.PostAsJsonAsync("/api/accounts", 
            new { accountNumber = "TEST004", initialBalance = 100 });

        var response = await _client.PostAsJsonAsync("/api/accounts/TEST004/deposit", 
            new { amount = 50 });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var account = await response.Content.ReadFromJsonAsync<AccountResponse>();
        Assert.Equal(150, account!.balance);
    }

    [Fact]
    public async Task Withdraw_WithSufficientFunds_ShouldDecreaseBalance()
    {
        await _client.PostAsJsonAsync("/api/accounts", 
            new { accountNumber = "TEST005", initialBalance = 100 });

        var response = await _client.PostAsJsonAsync("/api/accounts/TEST005/withdraw", 
            new { amount = 30 });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var account = await response.Content.ReadFromJsonAsync<AccountResponse>();
        Assert.Equal(70, account!.balance);
    }

    [Fact]
    public async Task Withdraw_WithInsufficientFunds_ShouldReturnBadRequest()
    {
        await _client.PostAsJsonAsync("/api/accounts", 
            new { accountNumber = "TEST006", initialBalance = 50 });

        var response = await _client.PostAsJsonAsync("/api/accounts/TEST006/withdraw", 
            new { amount = 100 });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Withdraw_WithOverdraft_WithinLimit_ShouldSucceed()
    {
        await _client.PostAsJsonAsync("/api/accounts", 
            new { accountNumber = "TEST021", initialBalance = 100, overdraftLimit = 200 });

        var response = await _client.PostAsJsonAsync("/api/accounts/TEST021/withdraw", 
            new { amount = 250 });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var account = await response.Content.ReadFromJsonAsync<AccountResponse>();
        Assert.Equal(-150, account!.balance);
    }

    [Fact]
    public async Task Withdraw_WithOverdraft_ExceedingLimit_ShouldReturnBadRequest()
    {
        await _client.PostAsJsonAsync("/api/accounts", 
            new { accountNumber = "TEST022", initialBalance = 100, overdraftLimit = 200 });

        var response = await _client.PostAsJsonAsync("/api/accounts/TEST022/withdraw", 
            new { amount = 350 });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task SetOverdraft_ShouldUpdateOverdraft()
    {
        await _client.PostAsJsonAsync("/api/accounts", 
            new { accountNumber = "TEST023", initialBalance = 100 });

        var response = await _client.PatchAsJsonAsync("/api/accounts/TEST023/overdraft", 
            new { overdraftLimit = 500 });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var account = await response.Content.ReadFromJsonAsync<AccountResponse>();
        Assert.Equal(500, account!.overdraftLimit);
    }

    [Fact]
    public async Task SetOverdraft_NonExistingAccount_ShouldReturnNotFound()
    {
        var response = await _client.PatchAsJsonAsync("/api/accounts/NONEXISTENT/overdraft", 
            new { overdraftLimit = 500 });

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetAllAccounts_ShouldReturnAllAccounts()
    {
        await _client.PostAsJsonAsync("/api/accounts", 
            new { accountNumber = "TEST007" });
        await _client.PostAsJsonAsync("/api/accounts", 
            new { accountNumber = "TEST008" });

        var response = await _client.GetAsync("/api/accounts");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var accounts = await response.Content.ReadFromJsonAsync<List<AccountResponse>>();
        Assert.True(accounts!.Count >= 2);
    }

    [Fact]
    public async Task GetStatement_ShouldReturnStatement()
    {
        await _client.PostAsJsonAsync("/api/accounts", 
            new { accountNumber = "TEST030", initialBalance = 100 });
        await _client.PostAsJsonAsync("/api/accounts/TEST030/deposit", 
            new { amount = 50 });
        await _client.PostAsJsonAsync("/api/accounts/TEST030/withdraw", 
            new { amount = 30 });

        var response = await _client.GetAsync("/api/accounts/TEST030/statement");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var statement = await response.Content.ReadFromJsonAsync<StatementResponse>();
        Assert.Equal("TEST030", statement!.accountNumber);
        Assert.Equal("Compte Courant", statement.accountType);
        Assert.Equal(120, statement.currentBalance);
        Assert.Equal(2, statement.operations.Count);
    }

    [Fact]
    public async Task GetStatement_NonExistingAccount_ShouldReturnNotFound()
    {
        var response = await _client.GetAsync("/api/accounts/NONEXISTENT/statement");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    private record AccountResponse(string accountNumber, decimal balance, decimal overdraftLimit);

    private record StatementResponse(
        string accountNumber, 
        string accountType, 
        decimal currentBalance, 
        DateTime statementDate,
        List<OperationResponse> operations
    );

    private record OperationResponse(Guid id, string accountNumber, decimal amount, string type, DateTime date, decimal balanceAfterTransaction);
}
