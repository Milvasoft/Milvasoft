using Cronos;
using System;

namespace Milvasoft.Helpers.JobScheduling;

/// <summary>
/// Schedule config for scheduling jobs.
/// </summary>
/// <typeparam name="T"></typeparam>
public interface IScheduleConfig<T>
{
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
    public CronFormat? CronFormat { get; set; }
}

/// <summary>
/// Schedule config for scheduling jobs.
/// </summary>
/// <typeparam name="T"></typeparam>
public class ScheduleConfig<T> : IScheduleConfig<T>
{
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
    public CronFormat? CronFormat { get; set; }
}
