using eVisaPlatform.Application.Interfaces;

namespace eVisaPlatform.API.BackgroundServices;

/// <summary>Periodically removes expired and revoked refresh tokens from storage.</summary>
public sealed class RefreshTokenCleanupService : BackgroundService
{
    private readonly IServiceProvider _services;
    private readonly ILogger<RefreshTokenCleanupService> _logger;

    public RefreshTokenCleanupService(
        IServiceProvider services,
        ILogger<RefreshTokenCleanupService> logger)
    {
        _services  = services;
        _logger    = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);

        await RunCleanupAsync(stoppingToken);

        using var timer = new PeriodicTimer(TimeSpan.FromHours(24));
        try
        {
            while (await timer.WaitForNextTickAsync(stoppingToken))
                await RunCleanupAsync(stoppingToken);
        }
        catch (OperationCanceledException)
        {
            // shutdown
        }
    }

    private async Task RunCleanupAsync(CancellationToken ct)
    {
        try
        {
            using var scope = _services.CreateScope();
            var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var removed = await uow.RefreshTokens.DeleteExpiredAndRevokedAsync(ct);
            if (removed > 0)
                _logger.LogInformation("Refresh token cleanup removed {Count} rows.", removed);
        }
        catch (Exception ex) when (!ct.IsCancellationRequested)
        {
            _logger.LogError(ex, "Refresh token cleanup failed.");
        }
    }
}
