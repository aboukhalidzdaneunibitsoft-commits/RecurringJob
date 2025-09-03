using RecurringJob.API.Entitys;
using RecurringJob.API.Enums;

namespace RecurringJob.API.IServices;

public interface ITimeTriggerService
{
    Task<TimeTrigger> CreateTriggerAsync(string name, string timeExpression, string methodName,
        TriggerType type = TriggerType.Cron, string? parameters = null);

    Task<List<TimeTrigger>> GetTriggersAsync();
    Task<TimeTrigger?> GetTriggerAsync(int id);
    Task<bool> DeleteTriggerAsync(int id);
    Task<bool> ToggleTriggerAsync(int id, bool isActive);

    Task ExecuteTriggersAsync();
    Task<List<TriggerExecutionLog>> GetExecutionLogsAsync(int triggerId, int take = 50);
}