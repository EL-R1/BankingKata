using BankingKata.Domain.Entities;
using BankingKata.Domain.Ports;

namespace BankingKata.Infrastructure.Persistence;

public class InMemorySavingsAccountRepository : ISavingsAccountRepository
{
    private readonly Dictionary<string, SavingsAccount> _accounts = new();

    public SavingsAccount? GetByAccountNumber(string accountNumber)
    {
        return _accounts.GetValueOrDefault(accountNumber);
    }

    public IEnumerable<SavingsAccount> GetAll()
    {
        return _accounts.Values;
    }

    public void Save(SavingsAccount account)
    {
        _accounts[account.AccountNumber] = account;
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
