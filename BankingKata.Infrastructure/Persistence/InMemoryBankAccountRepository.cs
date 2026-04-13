using BankingKata.Domain.Entities;
using BankingKata.Domain.Ports;

namespace BankingKata.Infrastructure.Persistence;

public class InMemoryBankAccountRepository : IBankAccountRepository
{
    private readonly Dictionary<string, BankAccount> _accounts = new();

    public BankAccount? GetByAccountNumber(string accountNumber)
    {
        return _accounts.GetValueOrDefault(accountNumber);
    }

    public IEnumerable<BankAccount> GetAll()
    {
        return _accounts.Values;
    }

    public void Save(BankAccount account)
    {
        _accounts[account.AccountNumber] = account;
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
