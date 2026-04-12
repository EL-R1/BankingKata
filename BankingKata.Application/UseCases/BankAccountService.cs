using BankingKata.Application.DTOs;
using BankingKata.Application.Ports;
using BankingKata.Domain.Entities;

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
        var accountDto = ToDto(account);
        _repository.Save(accountDto);
        return accountDto;
    }

    public BankAccountDto? GetAccount(string accountNumber)
    {
        return _repository.GetByAccountNumber(accountNumber);
    }

    public IEnumerable<BankAccountDto> GetAllAccounts()
    {
        return _repository.GetAll();
    }

    public BankAccountDto Deposit(string accountNumber, decimal amount)
    {
        var dto = _repository.GetByAccountNumber(accountNumber)
            ?? throw new InvalidOperationException($"Account {accountNumber} not found");

        var account = ToEntity(dto);
        account.Deposit(amount);
        
        var updatedDto = ToDto(account);
        _repository.Update(updatedDto);
        
        RecordTransaction(accountNumber, amount, TransactionType.Deposit, updatedDto.Balance);
        
        return updatedDto;
    }

    public BankAccountDto Withdraw(string accountNumber, decimal amount)
    {
        var dto = _repository.GetByAccountNumber(accountNumber)
            ?? throw new InvalidOperationException($"Account {accountNumber} not found");

        var account = ToEntity(dto);
        account.Withdraw(amount);
        
        var updatedDto = ToDto(account);
        _repository.Update(updatedDto);
        
        RecordTransaction(accountNumber, amount, TransactionType.Withdrawal, updatedDto.Balance);
        
        return updatedDto;
    }

    public BankAccountDto SetOverdraftLimit(string accountNumber, decimal limit)
    {
        var dto = _repository.GetByAccountNumber(accountNumber)
            ?? throw new InvalidOperationException($"Account {accountNumber} not found");

        var account = ToEntity(dto);
        account.SetOverdraftLimit(limit);
        
        var updatedDto = ToDto(account);
        _repository.Update(updatedDto);
        return updatedDto;
    }

    public StatementDto GetStatement(string accountNumber)
    {
        var dto = _repository.GetByAccountNumber(accountNumber)
            ?? throw new InvalidOperationException($"Account {accountNumber} not found");

        var toDate = DateTime.UtcNow;
        var fromDate = toDate.AddMonths(-1);

        var transactions = _transactionRepository.GetByAccountNumberInRange(accountNumber, fromDate, toDate);

        return new StatementDto(
            dto.AccountNumber,
            "Compte Courant",
            dto.Balance,
            toDate,
            transactions
        );
    }

    public StatementDto GetStatementInRange(string accountNumber, DateTime fromDate, DateTime toDate)
    {
        var dto = _repository.GetByAccountNumber(accountNumber)
            ?? throw new InvalidOperationException($"Account {accountNumber} not found");

        var transactions = _transactionRepository.GetByAccountNumberInRange(accountNumber, fromDate, toDate);

        return new StatementDto(
            dto.AccountNumber,
            "Compte Courant",
            dto.Balance,
            toDate,
            transactions
        );
    }

    private void RecordTransaction(string accountNumber, decimal amount, TransactionType type, decimal balanceAfter)
    {
        var operation = new OperationDto(
            Guid.NewGuid(),
            accountNumber,
            amount,
            type.ToString(),
            DateTime.UtcNow,
            balanceAfter
        );
        _transactionRepository.Save(operation);
    }

    private static BankAccountDto ToDto(BankAccount account) =>
        new(account.AccountNumber, account.Balance, account.OverdraftLimit);

    private static BankAccount ToEntity(BankAccountDto dto) =>
        new(dto.AccountNumber, dto.Balance, dto.OverdraftLimit);
}
