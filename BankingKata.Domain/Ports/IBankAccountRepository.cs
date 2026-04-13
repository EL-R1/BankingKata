using BankingKata.Domain.Entities;

namespace BankingKata.Domain.Ports;

public interface IBankAccountRepository
{
    BankAccount? GetByAccountNumber(string accountNumber);
    IEnumerable<BankAccount> GetAll();
    void Save(BankAccount account);
    void Update(BankAccount account);
    bool Exists(string accountNumber);
}
