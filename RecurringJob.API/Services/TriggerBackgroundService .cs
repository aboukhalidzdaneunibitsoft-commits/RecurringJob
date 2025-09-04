using RecurringJob.API.IServices;

namespace RecurringJob.API.Services;

public class TriggerBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<TriggerBackgroundService> _logger;

    public TriggerBackgroundService(
        IServiceProvider serviceProvider,
        ILogger<TriggerBackgroundService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Trigger Background Service started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var triggerService = scope.ServiceProvider.GetRequiredService<ITimeTriggerService>();

                await triggerService.ExecuteTriggersAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in trigger background service");
            }

            // Check every 30 seconds
            await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
        }

        _logger.LogInformation("Trigger Background Service stopped");
    }
}
