using System.Collections.Concurrent;
using BankingKata.Domain.Entities;
using BankingKata.Domain.Ports;

namespace BankingKata.Infrastructure.Persistence;

public class InMemoryBankAccountRepository : IBankAccountRepository
{
    private readonly ConcurrentDictionary<string, BankAccount> _accounts = new();

    public BankAccount? GetByAccountNumber(string accountNumber)
    {
        _accounts.TryGetValue(accountNumber, out var account);
        return account;
    }

    public IEnumerable<BankAccount> GetAll()
    {
        return _accounts.Values.Select(a => new BankAccount(a.AccountNumber, a.Balance, a.OverdraftLimit)).ToList();
    }

    public void Save(BankAccount account)
    {
        if (!_accounts.TryAdd(account.AccountNumber, account))
        {
            throw new InvalidOperationException($"Account {account.AccountNumber} already exists");
        }
    }

    public void Update(BankAccount account)
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
