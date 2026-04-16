using System.Net;
using System.Text.Json;
using BankingKata.Application.UseCases;
using BankingKata.Domain.Ports;
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

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("http://localhost:3000", "http://localhost:5173")
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseCors();

app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
        context.Response.ContentType = "application/json";

        var error = new
        {
            error = "InternalServerError",
            message = "An unexpected error occurred. Please try again later."
        };

        await context.Response.WriteAsJsonAsync(error);
    });
});

app.MapControllers();

app.Run();

public partial class Program { }
