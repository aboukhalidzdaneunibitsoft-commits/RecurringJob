namespace RecurringJob.API.Services;

using Microsoft.EntityFrameworkCore;
using NCrontab; // NuGet: NCrontab
using RecurringJob.API.Entitys;
using RecurringJob.API.Enums;
using RecurringJob.API.Databases;

using System.Diagnostics;
using System.Text.RegularExpressions;
using RecurringJob.API.IServices;

public class TimeTriggerService : ITimeTriggerService
    {
        private readonly AppDbContext _context;
        private readonly IMethodExecutor _methodExecutor;
        private readonly ILogger<TimeTriggerService> _logger;

        public TimeTriggerService(
            AppDbContext context,
            IMethodExecutor methodExecutor,
            ILogger<TimeTriggerService> logger)
        {
            _context = context;
            _methodExecutor = methodExecutor;
            _logger = logger;
        }

        public async Task<TimeTrigger> CreateTriggerAsync(string name, string timeExpression,
            string methodName, TriggerType type = TriggerType.Cron, string? parameters = null)
        {
            var trigger = new TimeTrigger
            {
                Name = name,
                TimeExpression = timeExpression,
                MethodName = methodName,
                TriggerType = type,
                Parameters = parameters
            };

          
            trigger.NextExecutionTime = CalculateNextExecution(trigger, DateTime.UtcNow);

            _context.TimeTriggers.Add(trigger);
            await _context.SaveChangesAsync();

            _logger.LogInformation("A new trigger has been created: {Name} – next execution: {NextTime}",
                name, trigger.NextExecutionTime);

            return trigger;
        }

        public async Task<List<TimeTrigger>> GetTriggersAsync()
        {
            return await _context.TimeTriggers.ToListAsync();
        }

        public async Task<TimeTrigger?> GetTriggerAsync(int id)
        {
            return await _context.TimeTriggers.FindAsync(id);
        }

        public async Task<bool> DeleteTriggerAsync(int id)
        {
            var trigger = await _context.TimeTriggers.FindAsync(id);
            if (trigger == null) return false;

            _context.TimeTriggers.Remove(trigger);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ToggleTriggerAsync(int id, bool isActive)
        {
            var trigger = await _context.TimeTriggers.FindAsync(id);
            if (trigger == null) return false;

            trigger.IsActive = isActive;
            if (isActive)
            {
                trigger.NextExecutionTime = CalculateNextExecution(trigger, DateTime.UtcNow);
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task ExecuteTriggersAsync()
        {
            var now = DateTime.UtcNow;

            var dueTriggersQuery = _context.TimeTriggers
                .Where(t => t.IsActive &&
                           t.NextExecutionTime.HasValue &&
                           t.NextExecutionTime.Value <= now);

            var dueTriggersCount = await dueTriggersQuery.CountAsync();
            if (dueTriggersCount == 0)
            {
                return; 
            }

            _logger.LogInformation("وُجد {Count} triggers مستحقة للتنفيذ", dueTriggersCount);

            var dueTriggers = await dueTriggersQuery.ToListAsync();

            foreach (var trigger in dueTriggers)
            {
                await ExecuteSingleTrigger(trigger, now);
            }

            await _context.SaveChangesAsync();
        }

        private async Task ExecuteSingleTrigger(TimeTrigger trigger, DateTime executionTime)
        {
            var stopwatch = Stopwatch.StartNew();
            var log = new TriggerExecutionLog
            {
                TriggerId = trigger.Id,
                ExecutedAt = executionTime
            };

            try
            {
                _logger.LogInformation("تنفيذ Trigger: {Name} - Method: {Method}",
                    trigger.Name, trigger.MethodName);

                var result = await _methodExecutor.ExecuteMethodAsync(
                    trigger.MethodName, trigger.Parameters);

                log.Success = true;
                log.Result = result?.ToString();

                trigger.LastExecutionTime = executionTime;
                trigger.NextExecutionTime = CalculateNextExecution(trigger, executionTime);

                _logger.LogInformation(" {Name}: {NextTime}",
                    trigger.Name, trigger.NextExecutionTime);
            }
            catch (Exception ex)
            {
                log.Success = false;
                log.ErrorMessage = ex.Message;

                _logger.LogError(ex, " Trigger: {Name}", trigger.Name);

                trigger.NextExecutionTime = CalculateNextExecution(trigger, executionTime);
            }
            finally
            {
                stopwatch.Stop();
                log.ExecutionDurationMs = (int)stopwatch.ElapsedMilliseconds;

                _context.TriggerExecutionLogs.Add(log);
            }
        }

        private DateTime? CalculateNextExecution(TimeTrigger trigger, DateTime fromTime)
        {
            try
            {
                switch (trigger.TriggerType)
                {
                    case TriggerType.Cron:
                        return CalculateCronNext(trigger.TimeExpression, fromTime);

                    case TriggerType.Simple:
                        return CalculateSimpleNext(trigger.TimeExpression, fromTime);

                    case TriggerType.Interval:
                        return CalculateIntervalNext(trigger.TimeExpression, fromTime);

                    default:
                        return null;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Trigger: {Name}", trigger.Name);
                return null;
            }
        }

        private DateTime? CalculateCronNext(string cronExpression, DateTime fromTime)
        {
            var crontab = CrontabSchedule.Parse(cronExpression);
            return crontab.GetNextOccurrence(fromTime);
        }

        private DateTime? CalculateSimpleNext(string expression, DateTime fromTime)
        {
            // Exemple:
            // "every monday 9:00"
            // "monthly 21 14:30" 
            // "daily 8:30"

            expression = expression.ToLowerInvariant().Trim();

        // exemple :  every monday 9:00
        var mondayMatch = Regex.Match(expression, @"every monday (\d{1,2}):(\d{2})");
            if (mondayMatch.Success)
            {
                var hour = int.Parse(mondayMatch.Groups[1].Value);
                var minute = int.Parse(mondayMatch.Groups[2].Value);
                return GetNextWeekday(fromTime, DayOfWeek.Monday, hour, minute);
            }

        //every  daily in time 8 : 30 
        var dailyMatch = Regex.Match(expression, @"daily (\d{1,2}):(\d{2})");
            if (dailyMatch.Success)
            {
                var hour = int.Parse(dailyMatch.Groups[1].Value);
                var minute = int.Parse(dailyMatch.Groups[2].Value);
                var today = fromTime.Date.AddHours(hour).AddMinutes(minute);
                return today > fromTime ? today : today.AddDays(1);
            }

            //
            var monthlyMatch = Regex.Match(expression, @"monthly (\d{1,2}) (\d{1,2}):(\d{2})");
            if (monthlyMatch.Success)
            {
                var day = int.Parse(monthlyMatch.Groups[1].Value);
                var hour = int.Parse(monthlyMatch.Groups[2].Value);
                var minute = int.Parse(monthlyMatch.Groups[3].Value);
                return GetNextMonthlyDate(fromTime, day, hour, minute);
            }

            return null;
        }

        private DateTime? CalculateIntervalNext(string expression, DateTime fromTime)
        {
        // exemples:
        // "every 30 minutes"
        // "every 2 hours"
        // "every 5 days"

        expression = expression.ToLowerInvariant().Trim();

            var minutesMatch = Regex.Match(expression, @"every (\d+) minutes?");
            if (minutesMatch.Success)
            {
                var minutes = int.Parse(minutesMatch.Groups[1].Value);
                return fromTime.AddMinutes(minutes);
            }

            var hoursMatch = Regex.Match(expression, @"every (\d+) hours?");
            if (hoursMatch.Success)
            {
                var hours = int.Parse(hoursMatch.Groups[1].Value);
                return fromTime.AddHours(hours);
            }

            var daysMatch = Regex.Match(expression, @"every (\d+) days?");
            if (daysMatch.Success)
            {
                var days = int.Parse(daysMatch.Groups[1].Value);
                return fromTime.AddDays(days);
            }

            return null;
        }

        private DateTime GetNextWeekday(DateTime fromDate, DayOfWeek targetDay, int hour, int minute)
        {
            var daysUntilTarget = ((int)targetDay - (int)fromDate.DayOfWeek + 7) % 7;
            if (daysUntilTarget == 0)
            {
            // Today is the target day, check time
            var todayTime = fromDate.Date.AddHours(hour).AddMinutes(minute);
                if (todayTime > fromDate)
                {
                    return todayTime;
                }
                daysUntilTarget = 7; // this week has passed, go to next week
        }

            return fromDate.Date.AddDays(daysUntilTarget).AddHours(hour).AddMinutes(minute);
        }

        private DateTime GetNextMonthlyDate(DateTime fromDate, int day, int hour, int minute)
        {
            var currentMonth = new DateTime(fromDate.Year, fromDate.Month, 1);

            for (int monthOffset = 0; monthOffset < 12; monthOffset++)
            {
                var targetMonth = currentMonth.AddMonths(monthOffset);
                var daysInMonth = DateTime.DaysInMonth(targetMonth.Year, targetMonth.Month);
                var actualDay = Math.Min(day, daysInMonth);

                var targetDate = new DateTime(targetMonth.Year, targetMonth.Month, actualDay, hour, minute, 0);

                if (targetDate > fromDate)
                {
                    return targetDate;
                }
            }

            return fromDate.AddMonths(1); // fallback
        }

        public async Task<List<TriggerExecutionLog>> GetExecutionLogsAsync(int triggerId, int take = 50)
        {
            return await _context.TriggerExecutionLogs
                .Where(log => log.TriggerId == triggerId)
                .OrderByDescending(log => log.ExecutedAt)
                .Take(take)
                .ToListAsync();
        }
}


