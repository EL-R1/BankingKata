using BankingKata.Application.DTOs;

namespace BankingKata.Application.Ports;

public interface IBankAccountRepository
{
    BankAccountDto? GetByAccountNumber(string accountNumber);
    IEnumerable<BankAccountDto> GetAll();
    void Save(BankAccountDto account);
    void Update(BankAccountDto account);
    bool Exists(string accountNumber);
}
