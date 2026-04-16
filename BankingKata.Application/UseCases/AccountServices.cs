using System.Reflection;
using BankingKata.Application.DTOs;
using BankingKata.Domain.Entities;
using BankingKata.Domain.Ports;

namespace BankingKata.Application.UseCases;

public abstract class AccountServiceBase<TAccount, TDto>
    where TAccount : class
    where TDto : class
{
    protected readonly ITransactionRepository _transactionRepository;

    protected AccountServiceBase(ITransactionRepository transactionRepository)
    {
        _transactionRepository = transactionRepository;
    }

    public abstract TDto? GetAccount(string accountNumber);
    public abstract IEnumerable<TDto> GetAllAccounts();
    public abstract StatementDto GetStatementInRange(string accountNumber, DateTime fromDate, DateTime toDate, string accountType);
    protected abstract TAccount? GetAccountEntity(string accountNumber);
    protected abstract void UpdateAccount(TAccount account);
    protected abstract void ValidateAccountExists(string accountNumber);
    protected abstract TDto ToDto(TAccount account);
    protected abstract decimal GetBalance(TAccount account);

    protected void PerformDeposit(string accountNumber, decimal amount)
    {
        var account = GetAccountEntity(accountNumber) 
            ?? throw new InvalidOperationException($"Account {accountNumber} not found");
        
        try
        {
            account.GetType().GetMethod("Deposit")!.Invoke(account, new object[] { amount });
        }
        catch (TargetInvocationException ex)
        {
            if (ex.InnerException != null)
                throw ex.InnerException;
            throw;
        }
        UpdateAccount(account);
        
        var transaction = Transaction.CreateDeposit(accountNumber, amount, GetBalance(account));
        _transactionRepository.Save(transaction);
    }

    protected void PerformWithdraw(string accountNumber, decimal amount)
    {
        var account = GetAccountEntity(accountNumber) 
            ?? throw new InvalidOperationException($"Account {accountNumber} not found");
        
        try
        {
            account.GetType().GetMethod("Withdraw")!.Invoke(account, new object[] { amount });
        }
        catch (TargetInvocationException ex)
        {
            if (ex.InnerException != null)
                throw ex.InnerException;
            throw;
        }
        UpdateAccount(account);
        
        var transaction = Transaction.CreateWithdrawal(accountNumber, amount, GetBalance(account));
        _transactionRepository.Save(transaction);
    }

    protected StatementDto GetStatementCore(string accountNumber, DateTime fromDate, DateTime toDate, string accountType)
    {
        ValidateAccountExists(accountNumber);
        var transactions = _transactionRepository.GetByAccountNumberInRange(accountNumber, fromDate, toDate);

        return new StatementDto(
            accountNumber,
            accountType,
            0,
            toDate,
            transactions.Select(ToOperationDto)
        );
    }

    protected static OperationDto ToOperationDto(Transaction t) =>
        new(t.Id, t.AccountNumber, t.Amount, t.Type.ToString(), t.Date, t.BalanceAfterTransaction);
}

public class BankAccountService : AccountServiceBase<BankAccount, BankAccountDto>
{
    private readonly IBankAccountRepository _repository;
    private readonly object _lock = new();

    public BankAccountService(IBankAccountRepository repository, ITransactionRepository transactionRepository)
        : base(transactionRepository)
    {
        _repository = repository;
    }

    public BankAccountDto CreateAccount(CreateAccountDto dto)
    {
        var account = new BankAccount(dto.AccountNumber, dto.InitialBalance, dto.OverdraftLimit);
        _repository.Save(account);
        return ToDto(account);
    }

    public override BankAccountDto? GetAccount(string accountNumber)
    {
        var account = _repository.GetByAccountNumber(accountNumber);
        return account is null ? null : ToDto(account);
    }

    public override IEnumerable<BankAccountDto> GetAllAccounts()
    {
        return _repository.GetAll().Select(ToDto);
    }

    public BankAccountDto Deposit(string accountNumber, decimal amount)
    {
        lock (_lock)
        {
            PerformDeposit(accountNumber, amount);
            return GetAccount(accountNumber)!;
        }
    }

    public BankAccountDto Withdraw(string accountNumber, decimal amount)
    {
        lock (_lock)
        {
            PerformWithdraw(accountNumber, amount);
            return GetAccount(accountNumber)!;
        }
    }

    public BankAccountDto SetOverdraftLimit(string accountNumber, decimal limit)
    {
        lock (_lock)
        {
            var account = _repository.GetByAccountNumber(accountNumber)
                ?? throw new InvalidOperationException($"Account {accountNumber} not found");

            account.SetOverdraftLimit(limit);
            _repository.Update(account);
            return ToDto(account);
        }
    }

    public StatementDto GetStatement(string accountNumber, DateTime? fromDate = null, DateTime? toDate = null)
    {
        var effectiveToDate = toDate ?? DateTime.UtcNow;
        var effectiveFromDate = fromDate ?? effectiveToDate.AddMonths(-1);

        var account = _repository.GetByAccountNumber(accountNumber)
            ?? throw new InvalidOperationException($"Account {accountNumber} not found");

        var statement = GetStatementCore(accountNumber, effectiveFromDate, effectiveToDate, AccountType.Checking);
        
        return new StatementDto(
            account.AccountNumber,
            AccountType.Checking,
            account.Balance,
            statement.StatementDate,
            statement.Operations
        );
    }

    public override StatementDto GetStatementInRange(string accountNumber, DateTime fromDate, DateTime toDate, string accountType)
    {
        var account = _repository.GetByAccountNumber(accountNumber)
            ?? throw new InvalidOperationException($"Account {accountNumber} not found");

        var statement = base.GetStatementCore(accountNumber, fromDate, toDate, accountType);
        
        return new StatementDto(
            account.AccountNumber,
            AccountType.Checking,
            account.Balance,
            statement.StatementDate,
            statement.Operations
        );
    }

    protected override BankAccount? GetAccountEntity(string accountNumber)
        => _repository.GetByAccountNumber(accountNumber);

    protected override void UpdateAccount(BankAccount account)
        => _repository.Update(account);

    protected override void ValidateAccountExists(string accountNumber)
    {
        if (!_repository.Exists(accountNumber))
            throw new InvalidOperationException($"Account {accountNumber} not found");
    }

    protected override BankAccountDto ToDto(BankAccount account)
        => new(account.AccountNumber, account.Balance, account.OverdraftLimit);

    protected override decimal GetBalance(BankAccount account)
        => account.Balance;
}

public class SavingsAccountService : AccountServiceBase<SavingsAccount, SavingsAccountDto>
{
    private readonly ISavingsAccountRepository _repository;
    private readonly object _lock = new();

    public SavingsAccountService(ISavingsAccountRepository repository, ITransactionRepository transactionRepository)
        : base(transactionRepository)
    {
        _repository = repository;
    }

    public SavingsAccountDto CreateAccount(CreateSavingsAccountDto dto)
    {
        var account = new SavingsAccount(dto.AccountNumber, dto.DepositCeiling, dto.InitialBalance);
        _repository.Save(account);
        return ToDto(account);
    }

    public override SavingsAccountDto? GetAccount(string accountNumber)
    {
        var account = _repository.GetByAccountNumber(accountNumber);
        return account is null ? null : ToDto(account);
    }

    public override IEnumerable<SavingsAccountDto> GetAllAccounts()
    {
        return _repository.GetAll().Select(ToDto);
    }

    public SavingsAccountDto Deposit(string accountNumber, decimal amount)
    {
        lock (_lock)
        {
            PerformDeposit(accountNumber, amount);
            return GetAccount(accountNumber)!;
        }
    }

    public SavingsAccountDto Withdraw(string accountNumber, decimal amount)
    {
        lock (_lock)
        {
            PerformWithdraw(accountNumber, amount);
            return GetAccount(accountNumber)!;
        }
    }

    public StatementDto GetStatement(string accountNumber, DateTime? fromDate = null, DateTime? toDate = null)
    {
        var effectiveToDate = toDate ?? DateTime.UtcNow;
        var effectiveFromDate = fromDate ?? effectiveToDate.AddMonths(-1);

        var account = _repository.GetByAccountNumber(accountNumber)
            ?? throw new InvalidOperationException($"Account {accountNumber} not found");

        var statement = GetStatementCore(accountNumber, effectiveFromDate, effectiveToDate, AccountType.Savings);
        
        return new StatementDto(
            account.AccountNumber,
            AccountType.Savings,
            account.Balance,
            statement.StatementDate,
            statement.Operations
        );
    }

    public override StatementDto GetStatementInRange(string accountNumber, DateTime fromDate, DateTime toDate, string accountType)
    {
        var account = _repository.GetByAccountNumber(accountNumber)
            ?? throw new InvalidOperationException($"Account {accountNumber} not found");

        var statement = base.GetStatementCore(accountNumber, fromDate, toDate, accountType);
        
        return new StatementDto(
            account.AccountNumber,
            AccountType.Savings,
            account.Balance,
            statement.StatementDate,
            statement.Operations
        );
    }

    protected override SavingsAccount? GetAccountEntity(string accountNumber)
        => _repository.GetByAccountNumber(accountNumber);

    protected override void UpdateAccount(SavingsAccount account)
        => _repository.Update(account);

    protected override void ValidateAccountExists(string accountNumber)
    {
        if (!_repository.Exists(accountNumber))
            throw new InvalidOperationException($"Account {accountNumber} not found");
    }

    protected override SavingsAccountDto ToDto(SavingsAccount account)
        => new(account.AccountNumber, account.Balance, account.DepositCeiling);

    protected override decimal GetBalance(SavingsAccount account)
        => account.Balance;
}
