using BankingKata.Application.DTOs;

namespace BankingKata.Application.Ports;

public interface ISavingsAccountRepository
{
    SavingsAccountDto? GetByAccountNumber(string accountNumber);
    IEnumerable<SavingsAccountDto> GetAll();
    void Save(SavingsAccountDto account);
    void Update(SavingsAccountDto account);
    bool Exists(string accountNumber);
}
