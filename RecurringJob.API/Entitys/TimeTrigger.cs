using RecurringJob.API.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RecurringJob.API.Entitys;

public class TimeTrigger : EntityBase
{
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    // التعبير الزمني - يمكن أن يكون cron أو تعبير بسيط
    [Required]
    [MaxLength(100)]
    public string TimeExpression { get; set; } = string.Empty;

    // نوع التعبير
    public TriggerType TriggerType { get; set; } = TriggerType.Cron;

    // اسم الدالة/المتود اللي بدها تتنفذ
    [Required]
    [MaxLength(200)]
    public string MethodName { get; set; } = string.Empty;

    // معاملات إضافية (JSON)
    public string? Parameters { get; set; }

    public bool IsActive { get; set; } = true;
    public DateTime? NextExecutionTime { get; set; }
    public DateTime? LastExecutionTime { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}