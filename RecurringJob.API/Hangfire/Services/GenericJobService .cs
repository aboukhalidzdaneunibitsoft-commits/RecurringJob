using Hangfire;
using RecurringJob.API.Databases;
using RecurringJob.API.Hangfire.Entitys;
using RecurringJob.API.Hangfire.IServices;
using RecurringJob.API.Hangfire.Models;
using System.Text.Json;

namespace RecurringJob.API.Hangfire.Services;

public class GenericJobService : IGenericJobService
{
    private readonly AppDbContext _context;
    private readonly IRecurringJobManager _recurringJobManager;
    private readonly ILogger<GenericJobService> _logger;

    public GenericJobService(
        AppDbContext context,
        IRecurringJobManager recurringJobManager,
        ILogger<GenericJobService> logger)
    {
        _context = context;
        _recurringJobManager = recurringJobManager;
        _logger = logger;
    }

    public async Task<TimeTrigger> CreateTriggerAsync(CreateTriggerDto dto, string? createdBy = null)
    { 

        var trigger = new TimeTrigger
        {
            CronExpression = dto.CronExpression,
            StartDate = dto.StartDate,
            EndDate = dto.EndDate,
            Timezone = dto.Timezone,
            JobType = dto.JobType,
            JobParameters = dto.JobParameters != null ? JsonSerializer.Serialize(dto.JobParameters) : null
        };

        _context.TimeTriggers.Add(trigger);
        await _context.SaveChangesAsync();

        // Create Hangfire recurring job
        if (trigger.IsActive)
        {
            await CreateHangfireJobAsync(trigger);
        }

        _logger.LogInformation("Created trigger: {Code} for site: {SiteId}", trigger.Code, trigger.SiteId);
        return trigger;
    }

    public async Task<TimeTrigger?> UpdateTriggerAsync(Guid id, UpdateTriggerDto dto, string? updatedBy = null)
    {
        var trigger = await _context.TimeTriggers.FindAsync(id);
        if (trigger == null) return null;

        var needsJobUpdate = false;

        // Update properties
        

        if (!string.IsNullOrEmpty(dto.CronExpression) && dto.CronExpression != trigger.CronExpression)
        {
            trigger. = dto.CronExpression;
            needsJobUpdate = true;
        }

        if (dto.StartDate.HasValue) trigger.StartDate = dto.StartDate;
        if (dto.EndDate.HasValue) trigger.EndDate = dto.EndDate;

        if (!string.IsNullOrEmpty(dto.Timezone) && dto.Timezone != trigger.Timezone)
        {
            trigger.Timezone = dto.Timezone;
            needsJobUpdate = true;
        }

        if (dto.JobParameters != null)
        {
            trigger.JobParameters = JsonSerializer.Serialize(dto.JobParameters);
            needsJobUpdate = true;
        }

        if (dto.IsActive.HasValue && dto.IsActive.Value != trigger.IsActive)
        {
            trigger.IsActive = dto.IsActive.Value;
            needsJobUpdate = true;
        }

        trigger.UpdatedBy = updatedBy;
        trigger.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        // Update Hangfire job if needed
        if (needsJobUpdate)
        {
            await UpdateHangfireJobAsync(trigger);
        }

        _logger.LogInformation("Updated trigger: {Code} for site: {SiteId}", trigger.Code, trigger.SiteId);
        return trigger;
    }

    public async Task<bool> DeleteTriggerAsync(Guid id)
    {
        var trigger = await _context.TimeTriggers.FindAsync(id);
        if (trigger == null) return false;

        // Remove Hangfire job
        await RemoveHangfireJobAsync(trigger);

        _context.TimeTriggers.Remove(trigger);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Deleted trigger: {Code} for site: {SiteId}", trigger.Code, trigger.SiteId);
        return true;
    }

    public async Task<TimeTrigger?> GetTriggerAsync(Guid id)
    {
        return await _context.TimeTriggers.FindAsync(id);
    }

    public async Task<List<TimeTrigger>> GetTriggersAsync(string? siteId = null, string? prefix = null, bool? isActive = null)
    {
        var query = _context.TimeTriggers.AsQueryable();

        if (!string.IsNullOrEmpty(siteId))
            query = query.Where(t => t.SiteId == siteId);

        if (!string.IsNullOrEmpty(prefix))
            query = query.Where(t => t.Prefix == prefix);

        if (isActive.HasValue)
            query = query.Where(t => t.IsActive == isActive.Value);

        return await query.OrderBy(t => t.Name).ToListAsync();
    }

    public async Task<bool> ActivateTriggerAsync(Guid id)
    {
        var trigger = await _context.TimeTriggers.FindAsync(id);
        if (trigger == null) return false;

        trigger.IsActive = true;
        trigger.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        await CreateHangfireJobAsync(trigger);
        return true;
    }

    public async Task<bool> DeactivateTriggerAsync(Guid id)
    {
        var trigger = await _context.TimeTriggers.FindAsync(id);
        if (trigger == null) return false;

        trigger.IsActive = false;
        trigger.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        await RemoveHangfireJobAsync(trigger);
        return true;
    }

    public async Task<List<TimeTriggerExecutionHistory>> GetExecutionHistoryAsync(Guid triggerId, int take = 50)
    {
        return await _context.TimeTriggerExecutionHistory
            .Where(h => h.TimeTriggerTriggerId == triggerId)
            .OrderByDescending(h => h.ExecutedAt)
            .Take(take)
            .ToListAsync();
    }

    // Private methods for Hangfire job management
    private async Task CreateHangfireJobAsync(TimeTrigger trigger)
    {
        try
        {
            var jobId = GetHangfireJobId(trigger);

            // Remove existing job if any
            if (!string.IsNullOrEmpty(trigger.ActivationJobId))
            {
                _recurringJobManager.RemoveIfExists(trigger.ActivationJobId);
            }

            // Create new recurring job
            _recurringJobManager.AddOrUpdate<IGenericJobExecutor>(
                jobId,
                executor => executor.ExecuteAsync(trigger.Id),
                trigger.CronExpression,
                TimeZoneInfo.FindSystemTimeZoneById(trigger.Timezone)
            );

            // Update trigger with job ID
            trigger.ActivationJobId = jobId;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Created Hangfire job: {JobId} for trigger: {Code}", jobId, trigger.Code);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create Hangfire job for trigger: {Code}", trigger.Code);
            throw;
        }
    }

    private async Task UpdateHangfireJobAsync(TimeTrigger trigger)
    {
        if (trigger.IsActive)
        {
            await CreateHangfireJobAsync(trigger);
        }
        else
        {
            await RemoveHangfireJobAsync(trigger);
        }
    }

    private async Task RemoveHangfireJobAsync(TimeTrigger trigger)
    {
        try
        {
            if (!string.IsNullOrEmpty(trigger.ActivationJobId))
            {
                _recurringJobManager.RemoveIfExists(trigger.ActivationJobId);
                trigger.ActivationJobId = null;
                await _context.SaveChangesAsync();

                _logger.LogInformation("Removed Hangfire job for trigger: {Code}", trigger.Code);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to remove Hangfire job for trigger: {Code}", trigger.Code);
        }
    }

    private static string GetHangfireJobId(TimeTrigger trigger)
    {
        return $"{trigger.SiteId}:{trigger.Prefix}:{trigger.Code}".Replace(" ", "_").ToLowerInvariant();
    }
}