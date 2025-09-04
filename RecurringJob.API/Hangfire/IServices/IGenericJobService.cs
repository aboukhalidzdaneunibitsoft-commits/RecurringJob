using RecurringJob.API.Hangfire.Entitys;
using RecurringJob.API.Hangfire.Models;

namespace RecurringJob.API.Hangfire.IServices;

public interface IGenericJobService
{
    Task<TimeTrigger> CreateTriggerAsync(CreateTriggerDto dto, string? createdBy = null);
    Task<TimeTrigger?> UpdateTriggerAsync(Guid id, UpdateTriggerDto dto, string? updatedBy = null);
    Task<bool> DeleteTriggerAsync(Guid id);

    Task<TimeTrigger?> GetTriggerAsync(Guid id);
    Task<List<TimeTrigger>> GetTriggersAsync(string? siteId = null, string? prefix = null, bool? isActive = null);

    Task<bool> ActivateTriggerAsync(Guid id);
    Task<bool> DeactivateTriggerAsync(Guid id);

    Task<List<TimeTriggerExecutionHistory>> GetExecutionHistoryAsync(Guid triggerId, int take = 50);
}
