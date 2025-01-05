using Cronos;

namespace Milvasoft.JobScheduling;

/// <summary>
/// Schedule config for scheduling jobs.
/// </summary>
public interface IScheduleConfig
{
    /// <summary>
    /// Use UTC date times.
    /// </summary>
    bool UseUtcDateTimes { get; set; }

    /// <summary>
    /// Cron expression of job.
    /// </summary>
    string CronExpression { get; set; }

    /// <summary>
    /// Time zone information of job.
    /// </summary>
    TimeZoneInfo TimeZoneInfo { get; set; }

    /// <summary>
    /// If seconds requested in cron expression send this property.
    /// </summary>
    public CronFormat CronFormat { get; set; }
}

/// <summary>
/// Schedule config for scheduling jobs.
/// </summary>
public class ScheduleConfig : IScheduleConfig
{
    /// <summary>
    /// Use UTC date times.
    /// </summary>
    public bool UseUtcDateTimes { get; set; }

    /// <summary>
    /// Cron expression of job.
    /// </summary>
    public string CronExpression { get; set; }

    /// <summary>
    /// Time zone information of job.
    /// </summary>
    public TimeZoneInfo TimeZoneInfo { get; set; }

    /// <summary>
    /// If seconds requested in cron expression send this property.
    /// </summary>
    public CronFormat CronFormat { get; set; } = CronFormat.IncludeSeconds;
}
