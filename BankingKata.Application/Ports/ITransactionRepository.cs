using BankingKata.Application.DTOs;

namespace BankingKata.Application.Ports;

public interface ITransactionRepository
{
    void Save(OperationDto transaction);
    IEnumerable<OperationDto> GetByAccountNumber(string accountNumber);
    IEnumerable<OperationDto> GetByAccountNumberInRange(string accountNumber, DateTime fromDate, DateTime toDate);
}
