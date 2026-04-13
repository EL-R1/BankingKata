using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using BankingKata.Application.Ports;
using BankingKata.Infrastructure.Persistence;

namespace BankingKata.Api.Tests;

public class TestWebApplicationFactory : WebApplicationFactory<Program>, IDisposable
{
    private readonly IServiceProvider _services;
    
    public TestWebApplicationFactory()
    {
        var serviceCollection = new ServiceCollection();
        
        serviceCollection.AddSingleton<IBankAccountRepository, InMemoryBankAccountRepository>();
        serviceCollection.AddSingleton<ITransactionRepository, InMemoryTransactionRepository>();
        serviceCollection.AddSingleton<ISavingsAccountRepository, InMemorySavingsAccountRepository>();
        
        _services = serviceCollection.BuildServiceProvider();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            services.AddSingleton(_services);
        });
    }

    public void ResetState()
    {
        var bankRepo = _services.GetRequiredService<IBankAccountRepository>() as InMemoryBankAccountRepository;
        var savingsRepo = _services.GetRequiredService<ISavingsAccountRepository>() as InMemorySavingsAccountRepository;
        var txRepo = _services.GetRequiredService<ITransactionRepository>() as InMemoryTransactionRepository;
        
        bankRepo?.Clear();
        savingsRepo?.Clear();
        txRepo?.Clear();
    }

    public new void Dispose()
    {
        if (_services is IDisposable disposable)
        {
            disposable.Dispose();
        }
        base.Dispose();
    }
}
