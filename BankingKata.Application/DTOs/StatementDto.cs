namespace BankingKata.Application.DTOs;

public record OperationDto(Guid Id, string AccountNumber, decimal Amount, string Type, DateTime Date, decimal BalanceAfterTransaction);

public record StatementDto(
    string AccountNumber,
    string AccountType,
    decimal CurrentBalance,
    DateTime StatementDate,
    IEnumerable<OperationDto> Operations
);

public record AccountStatementRequest(DateTime? FromDate = null, DateTime? ToDate = null);
