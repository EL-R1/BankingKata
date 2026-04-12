using BankingKata.Application.Ports;
using BankingKata.Application.UseCases;
using BankingKata.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<IBankAccountRepository, InMemoryBankAccountRepository>();
builder.Services.AddSingleton<ITransactionRepository, InMemoryTransactionRepository>();
builder.Services.AddScoped<BankAccountService>();

builder.Services.AddSingleton<ISavingsAccountRepository, InMemorySavingsAccountRepository>();
builder.Services.AddScoped<SavingsAccountService>();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseCors(policy => policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());

app.MapControllers();

app.Run();

public partial class Program { }
