namespace BankingKata.Application.DTOs;

public record SavingsAccountDto(string AccountNumber, decimal Balance, decimal DepositCeiling);

public record CreateSavingsAccountDto(string AccountNumber, decimal DepositCeiling, decimal InitialBalance = 0);

public record SavingsTransactionDto(decimal Amount);
