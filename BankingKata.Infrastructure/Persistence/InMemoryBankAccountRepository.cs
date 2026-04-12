using BankingKata.Application.DTOs;
using BankingKata.Application.Ports;

namespace BankingKata.Infrastructure.Persistence;

public class InMemoryBankAccountRepository : IBankAccountRepository
{
    private readonly Dictionary<string, BankAccountDto> _accounts = new();

    public BankAccountDto? GetByAccountNumber(string accountNumber)
    {
        return _accounts.GetValueOrDefault(accountNumber);
    }

    public IEnumerable<BankAccountDto> GetAll()
    {
        return _accounts.Values;
    }

    public void Save(BankAccountDto account)
    {
        _accounts[account.AccountNumber] = account;
    }

    public void Update(BankAccountDto account)
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
