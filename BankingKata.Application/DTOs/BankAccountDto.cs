namespace BankingKata.Application.DTOs;

public record BankAccountDto(string AccountNumber, decimal Balance, decimal OverdraftLimit);

public record CreateAccountDto(string AccountNumber, decimal InitialBalance = 0, decimal OverdraftLimit = 0);

public record TransactionDto(decimal Amount);

public record SetOverdraftDto(decimal OverdraftLimit);
