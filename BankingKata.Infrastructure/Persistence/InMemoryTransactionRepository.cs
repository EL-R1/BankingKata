using BankingKata.Application.DTOs;
using BankingKata.Application.Ports;

namespace BankingKata.Infrastructure.Persistence;

public class InMemoryTransactionRepository : ITransactionRepository
{
    private readonly List<OperationDto> _transactions = new();

    public void Save(OperationDto transaction)
    {
        _transactions.Add(transaction);
    }

    public IEnumerable<OperationDto> GetByAccountNumber(string accountNumber)
    {
        return _transactions
            .Where(t => t.AccountNumber == accountNumber)
            .OrderByDescending(t => t.Date);
    }

    public IEnumerable<OperationDto> GetByAccountNumberInRange(string accountNumber, DateTime fromDate, DateTime toDate)
    {
        return _transactions
            .Where(t => t.AccountNumber == accountNumber && t.Date >= fromDate && t.Date <= toDate)
            .OrderByDescending(t => t.Date);
    }
}
