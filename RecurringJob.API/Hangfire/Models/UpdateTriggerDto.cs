namespace RecurringJob.API.Hangfire.Models;

public class UpdateTriggerDto
{
    public string? CronExpression { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? Timezone { get; set; }
    public bool? IsActive { get; set; }
    public Dictionary<string, object>? JobParameters { get; set; }
}
