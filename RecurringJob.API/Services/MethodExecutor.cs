using RecurringJob.API.IServices;

namespace RecurringJob.API.Services;

public class MethodExecutor : IMethodExecutor
{
    private readonly ILogger<MethodExecutor> _logger;
    private readonly IServiceProvider _serviceProvider;

    public MethodExecutor(ILogger<MethodExecutor> logger, IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    public async Task<object?> ExecuteMethodAsync(string methodName, string? parameters)
    {
        _logger.LogInformation("Ex Method: {MethodName}", methodName);

        return methodName.ToLowerInvariant() switch
        {
            "cleanup_database" => await CleanupDatabase(parameters),
            "send_daily_report" => await SendDailyReport(parameters),
            "backup_files" => await BackupFiles(parameters),
            "update_cache" => await UpdateCache(parameters),
            _ => await ExecuteCustomMethod(methodName, parameters)
        };
    }

    private async Task<string> CleanupDatabase(string? parameters)
    {
        _logger.LogInformation("Starting database cleanup...");
        await Task.Delay(2000); // Simulate work
        return "Cleaned 150 old records from database";
    }

    private async Task<string> SendDailyReport(string? parameters)
    {
        _logger.LogInformation("Sending daily report...");
        await Task.Delay(1000);
        return "Daily report sent to 5 users";
    }

    private async Task<string> BackupFiles(string? parameters)
    {
        _logger.LogInformation("Starting file backup...");
        await Task.Delay(3000);
        return "Backup created for 25 files";
    }

    private async Task<string> UpdateCache(string? parameters)
    {
        _logger.LogInformation("Updating cache...");
        await Task.Delay(500);
        return "Updated 10 cache items";
    }

    private async Task<string> ExecuteCustomMethod(string methodName, string? parameters)
    {
        // Here you can add logic to execute custom methods
        _logger.LogWarning("Unknown method: {MethodName}", methodName);
        await Task.CompletedTask;
        return $"Executed {methodName} with parameters: {parameters ?? "none"}";
    }
}