using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RecurringJob.API.Entitys;

public class TriggerExecutionLog : EntityBase
{
    public int TriggerId { get; set; }
    [ForeignKey("TriggerId")]
    public virtual TimeTrigger Trigger { get; set; } = null!;

    public DateTime ExecutedAt { get; set; } = DateTime.UtcNow;
    public bool Success { get; set; }
    public string? Result { get; set; }
    public string? ErrorMessage { get; set; }
    public int ExecutionDurationMs { get; set; }
}
