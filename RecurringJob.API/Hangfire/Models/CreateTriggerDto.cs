namespace RecurringJob.API.Hangfire.Models;

public class CreateTriggerDto
{
    public string CronExpression { get; set; } = string.Empty;
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string Timezone { get; set; } = "UTC";
    public string JobType { get; set; } = string.Empty;
    public Dictionary<string, object>? JobParameters { get; set; }
}
