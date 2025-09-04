using RecurringJob.API.Entitys;
using System.ComponentModel.DataAnnotations;

namespace RecurringJob.API.Hangfire.Entitys;

public class TimeTrigger : EntityBase
{
    public string CronExpression { get; set; } = string.Empty;

    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }

    public string Timezone { get; set; } = "UTC";

    public string? ActivationJobId { get; set; } // Current Hangfire job ID

    [MaxLength(200)]
    public string? DeactivationJobId { get; set; } // Job to stop this trigger

    public bool IsActive { get; set; } = true;

    // Job execution details
    [Required]
    [MaxLength(200)]
    public string JobType { get; set; } = string.Empty; // Class name or identifier

    public string? JobParameters { get; set; } // JSON parameters

    // Navigation
    public virtual ICollection<TimeTriggerExecutionHistory> ExecutionHistory { get; set; } = new List<TimeTriggerExecutionHistory>();

}
