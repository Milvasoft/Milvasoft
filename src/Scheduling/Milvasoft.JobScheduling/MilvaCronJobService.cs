using Cronos;
using Microsoft.Extensions.Hosting;
using Milvasoft.Core.Helpers;

namespace Milvasoft.JobScheduling;

/// <summary>
/// Provides scheduled job operations. 
/// </summary>
/// <remarks>
/// Initializes new instances of <see cref="MilvaCronJobService"/>
/// </remarks>
/// <param name="scheduleConfig"></param>
public abstract class MilvaCronJobService(IScheduleConfig scheduleConfig) : IHostedService
{
    private Task _executingTask;
    private CancellationTokenSource _cts;
    private readonly IScheduleConfig _scheduleConfig = scheduleConfig;
    private readonly CronExpression _expression = CronExpression.Parse(scheduleConfig.CronExpression, scheduleConfig.CronFormat);

    /// <inheritdoc/>
    public Task StartAsync(CancellationToken cancellationToken)
    {
        _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

        _executingTask = ScheduleAndRunAsync(_cts.Token);

        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public Task StopAsync(CancellationToken cancellationToken)
    {
        if (_executingTask == null)
            return Task.CompletedTask;

        _cts.Cancel();
        _cts.Dispose();

        return Task.WhenAny(_executingTask, Task.Delay(Timeout.Infinite, cancellationToken));
    }

    /// <summary>
    /// Executes the job.
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public abstract Task ExecuteAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Schedules and runs the job.
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    protected virtual async Task ScheduleAndRunAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            var now = CommonHelper.GetDateTimeOffsetNow(_scheduleConfig.UseUtcDateTimes);
            var next = _expression.GetNextOccurrence(now, _scheduleConfig.TimeZoneInfo);

            if (!next.HasValue)
            {
                // Retry later
                await Task.Delay(TimeSpan.FromMinutes(5), cancellationToken);
                continue;
            }

            var delay = next.Value - now;

            while (delay > TimeSpan.FromHours(1) && !cancellationToken.IsCancellationRequested)
            {
                await Task.Delay(TimeSpan.FromHours(1), cancellationToken);

                now = CommonHelper.GetDateTimeOffsetNow(_scheduleConfig.UseUtcDateTimes);

                delay = next.Value - now;
            }

            if (delay > TimeSpan.Zero)
                await Task.Delay(delay, cancellationToken);

            if (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    await ExecuteAsync(cancellationToken);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"{_scheduleConfig.Name} failed! Message: {ex.Message}");
                }
            }
        }
    }
}
