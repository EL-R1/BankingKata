using BankingKata.Application.DTOs;
using BankingKata.Application.Ports;

namespace BankingKata.Infrastructure.Persistence;

public class InMemorySavingsAccountRepository : ISavingsAccountRepository
{
    private readonly Dictionary<string, SavingsAccountDto> _accounts = new();

    public SavingsAccountDto? GetByAccountNumber(string accountNumber)
    {
        return _accounts.GetValueOrDefault(accountNumber);
    }

    public IEnumerable<SavingsAccountDto> GetAll()
    {
        return _accounts.Values;
    }

    public void Save(SavingsAccountDto account)
    {
        _accounts[account.AccountNumber] = account;
    }

    public void Update(SavingsAccountDto account)
    {
        if (!_accounts.ContainsKey(account.AccountNumber))
            throw new InvalidOperationException($"Account {account.AccountNumber} not found");
        _accounts[account.AccountNumber] = account;
    }

    public bool Exists(string accountNumber)
    {
        return _accounts.ContainsKey(accountNumber);
    }
}
