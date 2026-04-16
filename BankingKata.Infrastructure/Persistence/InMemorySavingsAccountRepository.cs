using System.Collections.Concurrent;
using BankingKata.Domain.Entities;
using BankingKata.Domain.Ports;

namespace BankingKata.Infrastructure.Persistence;

public class InMemorySavingsAccountRepository : ISavingsAccountRepository
{
    private readonly ConcurrentDictionary<string, SavingsAccount> _accounts = new();

    public SavingsAccount? GetByAccountNumber(string accountNumber)
    {
        _accounts.TryGetValue(accountNumber, out var account);
        return account;
    }

    public IEnumerable<SavingsAccount> GetAll()
    {
        return _accounts.Values.Select(a => new SavingsAccount(a.AccountNumber, a.DepositCeiling, a.Balance)).ToList();
    }

    public void Save(SavingsAccount account)
    {
        if (!_accounts.TryAdd(account.AccountNumber, account))
        {
            throw new InvalidOperationException($"Account {account.AccountNumber} already exists");
        }
    }

    public void Update(SavingsAccount account)
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
