using BankingKata.Application.DTOs;
using BankingKata.Application.Ports;
using BankingKata.Domain.Entities;

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
        var accountDto = ToDto(account);
        _repository.Save(accountDto);
        return accountDto;
    }

    public SavingsAccountDto? GetAccount(string accountNumber)
    {
        return _repository.GetByAccountNumber(accountNumber);
    }

    public IEnumerable<SavingsAccountDto> GetAllAccounts()
    {
        return _repository.GetAll();
    }

    public SavingsAccountDto Deposit(string accountNumber, decimal amount)
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

    public SavingsAccountDto Withdraw(string accountNumber, decimal amount)
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

    public StatementDto GetStatement(string accountNumber)
    {
        var dto = _repository.GetByAccountNumber(accountNumber)
            ?? throw new InvalidOperationException($"Account {accountNumber} not found");

        var toDate = DateTime.UtcNow;
        var fromDate = toDate.AddMonths(-1);

        var transactions = _transactionRepository.GetByAccountNumberInRange(accountNumber, fromDate, toDate);

        return new StatementDto(
            dto.AccountNumber,
            "Livret",
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
            "Livret",
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

    private static SavingsAccountDto ToDto(SavingsAccount account) =>
        new(account.AccountNumber, account.Balance, account.DepositCeiling);

    private static SavingsAccount ToEntity(SavingsAccountDto dto) =>
        new(dto.AccountNumber, dto.DepositCeiling, dto.Balance);
}
