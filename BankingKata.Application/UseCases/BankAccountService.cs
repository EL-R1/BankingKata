using BankingKata.Application.DTOs;
using BankingKata.Domain.Entities;
using BankingKata.Domain.Ports;

namespace BankingKata.Application.UseCases;

public class BankAccountService
{
    private readonly IBankAccountRepository _repository;
    private readonly ITransactionRepository _transactionRepository;

    public BankAccountService(IBankAccountRepository repository, ITransactionRepository transactionRepository)
    {
        _repository = repository;
        _transactionRepository = transactionRepository;
    }

    public BankAccountDto CreateAccount(CreateAccountDto dto)
    {
        if (_repository.Exists(dto.AccountNumber))
            throw new InvalidOperationException($"Account {dto.AccountNumber} already exists");

        var account = new BankAccount(dto.AccountNumber, dto.InitialBalance, dto.OverdraftLimit);
        _repository.Save(account);
        return ToDto(account);
    }

    public BankAccountDto? GetAccount(string accountNumber)
    {
        var account = _repository.GetByAccountNumber(accountNumber);
        return account is null ? null : ToDto(account);
    }

    public IEnumerable<BankAccountDto> GetAllAccounts()
    {
        return _repository.GetAll().Select(ToDto);
    }

    public BankAccountDto Deposit(string accountNumber, decimal amount)
    {
        var account = _repository.GetByAccountNumber(accountNumber)
            ?? throw new InvalidOperationException($"Account {accountNumber} not found");

        account.Deposit(amount);
        _repository.Update(account);
        
        RecordTransaction(accountNumber, amount, TransactionType.Deposit, account.Balance);
        
        return ToDto(account);
    }

    public BankAccountDto Withdraw(string accountNumber, decimal amount)
    {
        var account = _repository.GetByAccountNumber(accountNumber)
            ?? throw new InvalidOperationException($"Account {accountNumber} not found");

        account.Withdraw(amount);
        _repository.Update(account);
        
        RecordTransaction(accountNumber, amount, TransactionType.Withdrawal, account.Balance);
        
        return ToDto(account);
    }

    public BankAccountDto SetOverdraftLimit(string accountNumber, decimal limit)
    {
        var account = _repository.GetByAccountNumber(accountNumber)
            ?? throw new InvalidOperationException($"Account {accountNumber} not found");

        account.SetOverdraftLimit(limit);
        _repository.Update(account);
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
            "Compte Courant",
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
            "Compte Courant",
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

    private static BankAccountDto ToDto(BankAccount account) =>
        new(account.AccountNumber, account.Balance, account.OverdraftLimit);

    private static OperationDto ToOperationDto(Transaction t) =>
        new(t.Id, t.AccountNumber, t.Amount, t.Type.ToString(), t.Date, t.BalanceAfterTransaction);
}
