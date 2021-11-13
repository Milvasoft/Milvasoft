using Cronos;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Milvasoft.Helpers.JobScheduling;

/// <summary>
/// Provides scheduled job operations.
/// </summary>
public abstract class MilvaCronJobService : IHostedService, IDisposable
{
    private System.Timers.Timer _timer;
    private readonly CronExpression _expression;
    private readonly TimeZoneInfo _timeZoneInfo;

    /// <summary>
    /// Initializes new instances of <see cref="MilvaCronJobService"/>
    /// </summary>
    /// <param name="cronExpression"></param>
    /// <param name="timeZoneInfo"></param>
    protected MilvaCronJobService(string cronExpression, TimeZoneInfo timeZoneInfo)
    {
        _expression = CronExpression.Parse(cronExpression, CronFormat.IncludeSeconds);
        _timeZoneInfo = timeZoneInfo;
    }

    /// <summary>
    /// Initializes new instances of <see cref="MilvaCronJobService"/>
    /// </summary>
    /// <param name="cronExpression"></param>
    /// <param name="timeZoneInfo"></param>
    /// <param name="cronFormat"></param>
    protected MilvaCronJobService(string cronExpression, TimeZoneInfo timeZoneInfo, CronFormat cronFormat)
    {
        _expression = CronExpression.Parse(cronExpression, cronFormat);
        _timeZoneInfo = timeZoneInfo;
    }

    /// <summary>
    /// Starts the job.
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public virtual async Task StartAsync(CancellationToken cancellationToken) => await ScheduleJob(cancellationToken);

    /// <summary>
    /// Schedules the job.
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    protected virtual async Task ScheduleJob(CancellationToken cancellationToken)
    {
        var next = _expression.GetNextOccurrence(DateTimeOffset.Now, _timeZoneInfo);
        if (next.HasValue)
        {
            var delay = next.Value - DateTimeOffset.Now;
            if (delay.TotalMilliseconds <= 0)   // prevent non-positive values from being passed into Timer
            {
                await ScheduleJob(cancellationToken);
            }
            _timer = new System.Timers.Timer(delay.TotalMilliseconds);
            _timer.Elapsed += async (sender, args) =>
            {
                _timer.Dispose();  // reset and dispose timer
                _timer = null;

                if (!cancellationToken.IsCancellationRequested)
                {
                    await ExecuteAsync(cancellationToken);
                }

                if (!cancellationToken.IsCancellationRequested)
                {
                    await ScheduleJob(cancellationToken);    // reschedule next
                }
            };
            _timer.Start();
        }
        await Task.CompletedTask;
    }

    /// <summary>
    /// Executes the job.
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public virtual async Task ExecuteAsync(CancellationToken cancellationToken) => await Task.Delay(5000, cancellationToken);

    /// <summary>
    /// Stops the job.
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public virtual async Task StopAsync(CancellationToken cancellationToken)
    {
        _timer?.Stop();
        await Task.CompletedTask;
    }

    /// <summary>
    /// Disposes the timer.
    /// </summary>
    public virtual void Dispose() => _timer?.Dispose();
}
