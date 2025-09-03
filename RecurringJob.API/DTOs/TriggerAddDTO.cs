using RecurringJob.API.Enums;

namespace RecurringJob.API.DTOs;

public class TriggerAddDTO
{
    public string Name { get; set; } = string.Empty;
    public string TimeExpression { get; set; } = string.Empty;
    public string MethodName { get; set; } = string.Empty;
    public TriggerType TriggerType { get; set; } = TriggerType.Cron;
    public string? Parameters { get; set; }
}
