using RecurringJob.API.Entitys;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RecurringJob.API.Hangfire.Entitys;

public class TimeTriggerExecutionHistory : EntityBase
{
    public int TimeTriggerTriggerId { get; set; } 

    [ForeignKey(nameof(TimeTriggerTriggerId))]
    public virtual TimeTrigger TimeTrigger { get; set; } = null!;

    public DateTime ExecutedAt { get; set; } = DateTime.UtcNow;
    public bool Success { get; set; }
    public string? Result { get; set; }
    public string? ErrorMessage { get; set; }
    public int DurationMs { get; set; }

    [MaxLength(200)]
    public string? HangfireJobId { get; set; }
}
