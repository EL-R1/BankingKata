using BankingKata.Domain.Entities;

namespace BankingKata.Domain.Ports;

public interface ITransactionRepository
{
    void Save(Transaction transaction);
    IEnumerable<Transaction> GetByAccountNumber(string accountNumber);
    IEnumerable<Transaction> GetByAccountNumberInRange(string accountNumber, DateTime fromDate, DateTime toDate);
}
