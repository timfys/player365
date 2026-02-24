using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SmartWinners.Services;

public class SupportedLanguagesSyncHostedService : IHostedService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<SupportedLanguagesSyncHostedService> _logger;

    public SupportedLanguagesSyncHostedService(
        IServiceScopeFactory scopeFactory,
        ILogger<SupportedLanguagesSyncHostedService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting SupportedLanguagesSyncHostedService...");

        try
        {
            using var scope = _scopeFactory.CreateScope();
            var languageSyncService = scope.ServiceProvider.GetRequiredService<ILanguageSyncService>();
            await languageSyncService.SyncLanguagesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in SupportedLanguagesSyncHostedService");
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
