using Microsoft.AspNetCore.Mvc;
using BankingKata.Application.DTOs;
using BankingKata.Application.UseCases;

namespace BankingKata.Api.Controllers;

[ApiController]
[Route("api/savings")]
public class SavingsController : ControllerBase
{
    private readonly SavingsAccountService _service;

    public SavingsController(SavingsAccountService service)
    {
        _service = service;
    }

    [HttpGet]
    public ActionResult<IEnumerable<SavingsAccountDto>> GetAll()
    {
        var accounts = _service.GetAllAccounts().ToList();
        return accounts.Count == 0 ? NoContent() : Ok(accounts);
    }

    [HttpGet("{accountNumber}")]
    public ActionResult<SavingsAccountDto> Get(string accountNumber)
    {
        var account = _service.GetAccount(accountNumber);
        if (account is null)
            return NotFound(new { message = $"Account {accountNumber} not found" });
        return Ok(account);
    }

    [HttpPost]
    public ActionResult<SavingsAccountDto> Create([FromBody] CreateSavingsAccountDto dto)
    {
        try
        {
            var account = _service.CreateAccount(dto);
            return CreatedAtAction(nameof(Get), new { accountNumber = account.AccountNumber }, account);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("{accountNumber}/deposit")]
    public ActionResult<SavingsAccountDto> Deposit(string accountNumber, [FromBody] SavingsTransactionDto dto)
    {
        try
        {
            var account = _service.Deposit(accountNumber, dto.Amount);
            return Ok(account);
        }
        catch (InvalidOperationException ex)
        {
            if (ex.Message.Contains("not found"))
                return NotFound(new { message = ex.Message });
            return BadRequest(new { message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("{accountNumber}/withdraw")]
    public ActionResult<SavingsAccountDto> Withdraw(string accountNumber, [FromBody] SavingsTransactionDto dto)
    {
        try
        {
            var account = _service.Withdraw(accountNumber, dto.Amount);
            return Ok(account);
        }
        catch (InvalidOperationException ex)
        {
            if (ex.Message.Contains("not found"))
                return NotFound(new { message = ex.Message });
            return BadRequest(new { message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("{accountNumber}/statement")]
    public ActionResult<StatementDto> GetStatement(string accountNumber, [FromQuery] DateTime? fromDate, [FromQuery] DateTime? toDate)
    {
        try
        {
            var statement = _service.GetStatement(accountNumber, fromDate, toDate);
            return Ok(statement);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }
}
