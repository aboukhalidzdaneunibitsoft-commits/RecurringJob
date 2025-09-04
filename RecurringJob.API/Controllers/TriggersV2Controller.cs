using Microsoft.AspNetCore.Mvc;
using RecurringJob.API.Entitys;
using RecurringJob.API.Enums;
using RecurringJob.API.IServices;

namespace RecurringJob.API.Controllers;


    [ApiController]
    [Route("api/[controller]")]
    public class Triggersv2Controller : ControllerBase
    {
        private readonly ITimeTriggerService _triggerService;
        private readonly ILogger<TriggersController> _logger;

        public Triggersv2Controller(ITimeTriggerService triggerService, ILogger<TriggersController> logger)
        {
            _triggerService = triggerService;
            _logger = logger;
        }

        /// <summary>
        /// Create a new trigger
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<TimeTrigger>> CreateTrigger([FromBody] CreateTriggerRequest request)
        {
            try
            {
                var trigger = await _triggerService.CreateTriggerAsync(
                    request.Name,
                    request.TimeExpression,
                    request.MethodName,
                    request.TriggerType,
                    request.Parameters
                );

                return CreatedAtAction(nameof(GetTrigger), new { id = trigger.Id }, trigger);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating trigger");
                return BadRequest($"Error creating trigger: {ex.Message}");
            }
        }

        /// <summary>
        /// Get all triggers
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<List<TimeTrigger>>> GetTriggers()
        {
            var triggers = await _triggerService.GetTriggersAsync();
            return Ok(triggers);
        }

        /// <summary>
        /// Get specific trigger
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<TimeTrigger>> GetTrigger(int id)
        {
            var trigger = await _triggerService.GetTriggerAsync(id);
            return trigger == null ? NotFound() : Ok(trigger);
        }

        /// <summary>
        /// Get trigger execution logs
        /// </summary>
        [HttpGet("{id}/logs")]
        public async Task<ActionResult<List<TriggerExecutionLog>>> GetExecutionLogs(int id, [FromQuery] int take = 20)
        {
            var logs = await _triggerService.GetExecutionLogsAsync(id, take);
            return Ok(logs);
        }

        /// <summary>
        /// Toggle trigger active/inactive
        /// </summary>
        [HttpPut("{id}/toggle")]
        public async Task<ActionResult> ToggleTrigger(int id, [FromBody] bool isActive)
        {
            var success = await _triggerService.ToggleTriggerAsync(id, isActive);
            return success ? Ok($"Trigger {(isActive ? "activated" : "deactivated")}") : NotFound();
        }

        /// <summary>
        /// Delete trigger
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteTrigger(int id)
        {
            var success = await _triggerService.DeleteTriggerAsync(id);
            return success ? NoContent() : NotFound();
        }

        /// <summary>
        /// Create sample triggers for testing
        /// </summary>
        [HttpPost("samples")]
        public async Task<ActionResult<List<TimeTrigger>>> CreateSampleTriggers()
        {
            var samples = new List<TimeTrigger>();

            try
            {
                // 1. Every 2 minutes (for quick testing)
                samples.Add(await _triggerService.CreateTriggerAsync(
                    "Quick Test - Every 2 Minutes",
                    "every 2 minutes",
                    "update_cache",
                    TriggerType.Interval,
                    null
                ));

                // 2. Every Monday at 9:00 AM
                samples.Add(await _triggerService.CreateTriggerAsync(
                    "Weekly Database Cleanup",
                    "every monday 9:00",
                    "cleanup_database",
                    TriggerType.Simple,
                    "{\"retentionDays\": 30}"
                ));

                // 3. Daily at 8:30 AM
                samples.Add(await _triggerService.CreateTriggerAsync(
                    "Daily Report",
                    "daily 8:30",
                    "send_daily_report",
                    TriggerType.Simple,
                    "{\"recipients\": [\"admin@company.com\", \"manager@company.com\"]}"
                ));

                // 4. Monthly on 15th at 2:00 PM
                samples.Add(await _triggerService.CreateTriggerAsync(
                    "Monthly Backup",
                    "monthly 15 14:00",
                    "backup_files",
                    TriggerType.Simple,
                    "{\"backupPath\": \"/backups/monthly\"}"
                ));

                // 5. Cron: Every day at 2:30 AM
                samples.Add(await _triggerService.CreateTriggerAsync(
                    "Night Maintenance",
                    "30 2 * * *",
                    "cleanup_database",
                    TriggerType.Cron,
                    "{\"deep\": true}"
                ));

                // 6. Every 5 minutes (for testing)
                samples.Add(await _triggerService.CreateTriggerAsync(
                    "Frequent Cache Update",
                    "every 5 minutes",
                    "update_cache",
                    TriggerType.Interval,
                    "{\"cacheType\": \"user_data\"}"
                ));

                _logger.LogInformation("Created {Count} sample triggers", samples.Count);
                return Ok(samples);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating sample triggers");
                return BadRequest($"Error creating samples: {ex.Message}");
            }
        }

        /// <summary>
        /// Get dashboard info
        /// </summary>
        [HttpGet("dashboard")]
        public async Task<ActionResult<object>> GetDashboard()
        {
            var triggers = await _triggerService.GetTriggersAsync();

            var dashboard = new
            {
                TotalTriggers = triggers.Count,
                ActiveTriggers = triggers.Count(t => t.IsActive),
                InactiveTriggers = triggers.Count(t => !t.IsActive),
                NextExecution = triggers
                    .Where(t => t.IsActive && t.NextExecutionTime.HasValue)
                    .OrderBy(t => t.NextExecutionTime)
                    .Take(5)
                    .Select(t => new {
                        t.Name,
                        t.NextExecutionTime,
                        t.MethodName,
                        t.TimeExpression
                    }),
                RecentTriggers = triggers
                    .OrderByDescending(t => t.CreatedAt)
                    .Take(5)
                    .Select(t => new {
                        t.Name,
                        t.CreatedAt,
                        t.IsActive,
                        t.TimeExpression
                    })
            };

            return Ok(dashboard);
        }
    }

    public class CreateTriggerRequest
    {
        public string Name { get; set; } = string.Empty;
        public string TimeExpression { get; set; } = string.Empty;
        public string MethodName { get; set; } = string.Empty;
        public TriggerType TriggerType { get; set; } = TriggerType.Simple;
        public string? Parameters { get; set; }
    }

