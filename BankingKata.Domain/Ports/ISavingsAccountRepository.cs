using BankingKata.Domain.Entities;

namespace BankingKata.Domain.Ports;

public interface ISavingsAccountRepository
{
    SavingsAccount? GetByAccountNumber(string accountNumber);
    IEnumerable<SavingsAccount> GetAll();
    void Save(SavingsAccount account);
    void Update(SavingsAccount account);
    bool Exists(string accountNumber);
}
