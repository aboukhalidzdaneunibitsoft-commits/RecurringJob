namespace RecurringJob.API.Enums;

public enum TriggerType
{
    Cron,           // "0 0 * * 1"
    Simple,         // "every monday 9:00", "monthly 21 14:30"
    Interval        // "every 30 minutes", "every 2 hours"
}


