using System.Collections.Concurrent;
using BankingKata.Domain.Entities;
using BankingKata.Domain.Ports;

namespace BankingKata.Infrastructure.Persistence;

public class InMemoryTransactionRepository : ITransactionRepository
{
    private readonly ConcurrentBag<Transaction> _transactions = new();

    public void Save(Transaction transaction)
    {
        _transactions.Add(transaction);
    }

    public IEnumerable<Transaction> GetByAccountNumber(string accountNumber)
    {
        return _transactions
            .Where(t => t.AccountNumber == accountNumber)
            .OrderByDescending(t => t.Date)
            .ToList();
    }

    public IEnumerable<Transaction> GetByAccountNumberInRange(string accountNumber, DateTime fromDate, DateTime toDate)
    {
        return _transactions
            .Where(t => t.AccountNumber == accountNumber && t.Date >= fromDate && t.Date <= toDate)
            .OrderByDescending(t => t.Date)
            .ToList();
    }
}
