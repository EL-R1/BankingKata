using BankingKata.Application.DTOs;
using BankingKata.Domain.Entities;
using BankingKata.Domain.Ports;

namespace BankingKata.Application.UseCases;

public class SavingsAccountService
{
    private readonly ISavingsAccountRepository _repository;
    private readonly ITransactionRepository _transactionRepository;

    public SavingsAccountService(ISavingsAccountRepository repository, ITransactionRepository transactionRepository)
    {
        _repository = repository;
        _transactionRepository = transactionRepository;
    }

    public SavingsAccountDto CreateAccount(CreateSavingsAccountDto dto)
    {
        if (_repository.Exists(dto.AccountNumber))
            throw new InvalidOperationException($"Account {dto.AccountNumber} already exists");

        var account = new SavingsAccount(dto.AccountNumber, dto.DepositCeiling, dto.InitialBalance);
        _repository.Save(account);
        return ToDto(account);
    }

    public SavingsAccountDto? GetAccount(string accountNumber)
    {
        var account = _repository.GetByAccountNumber(accountNumber);
        return account is null ? null : ToDto(account);
    }

    public IEnumerable<SavingsAccountDto> GetAllAccounts()
    {
        return _repository.GetAll().Select(ToDto);
    }

    public SavingsAccountDto Deposit(string accountNumber, decimal amount)
    {
        var account = _repository.GetByAccountNumber(accountNumber)
            ?? throw new InvalidOperationException($"Account {accountNumber} not found");

        account.Deposit(amount);
        _repository.Update(account);
        
        RecordTransaction(accountNumber, amount, TransactionType.Deposit, account.Balance);
        
        return ToDto(account);
    }

    public SavingsAccountDto Withdraw(string accountNumber, decimal amount)
    {
        var account = _repository.GetByAccountNumber(accountNumber)
            ?? throw new InvalidOperationException($"Account {accountNumber} not found");

        account.Withdraw(amount);
        _repository.Update(account);
        
        RecordTransaction(accountNumber, amount, TransactionType.Withdrawal, account.Balance);
        
        return ToDto(account);
    }

    public StatementDto GetStatement(string accountNumber)
    {
        var account = _repository.GetByAccountNumber(accountNumber)
            ?? throw new InvalidOperationException($"Account {accountNumber} not found");

        var toDate = DateTime.UtcNow;
        var fromDate = toDate.AddMonths(-1);

        var transactions = _transactionRepository.GetByAccountNumberInRange(accountNumber, fromDate, toDate);

        return new StatementDto(
            account.AccountNumber,
            "Livret",
            account.Balance,
            toDate,
            transactions.Select(ToOperationDto)
        );
    }

    public StatementDto GetStatementInRange(string accountNumber, DateTime fromDate, DateTime toDate)
    {
        var account = _repository.GetByAccountNumber(accountNumber)
            ?? throw new InvalidOperationException($"Account {accountNumber} not found");

        var transactions = _transactionRepository.GetByAccountNumberInRange(accountNumber, fromDate, toDate);

        return new StatementDto(
            account.AccountNumber,
            "Livret",
            account.Balance,
            toDate,
            transactions.Select(ToOperationDto)
        );
    }

    private void RecordTransaction(string accountNumber, decimal amount, TransactionType type, decimal balanceAfter)
    {
        var transaction = new Transaction(accountNumber, amount, type, balanceAfter);
        _transactionRepository.Save(transaction);
    }

    private static SavingsAccountDto ToDto(SavingsAccount account) =>
        new(account.AccountNumber, account.Balance, account.DepositCeiling);

    private static OperationDto ToOperationDto(Transaction t) =>
        new(t.Id, t.AccountNumber, t.Amount, t.Type.ToString(), t.Date, t.BalanceAfterTransaction);
}
