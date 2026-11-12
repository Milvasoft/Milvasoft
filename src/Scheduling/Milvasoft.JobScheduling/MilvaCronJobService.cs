using Cronos;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Milvasoft.Core.Helpers;

namespace Milvasoft.JobScheduling;

/// <summary>
/// Provides scheduled job operations.
/// </summary>
/// <remarks>
/// <para>When <see cref="IScheduleConfig.TimeZoneInfo"/> is null, occurrences are calculated in UTC if <see cref="IScheduleConfig.UseUtcDateTimes"/> is true,
/// otherwise in the local time zone (Cronos itself does not accept a null time zone).</para>
/// <para>Every occurrence runs at most once: the next occurrence is always calculated after the previous one, so a timer that fires slightly early
/// cannot run the same occurrence twice.</para>
/// <para>Job failures are logged with the given <see cref="ILogger"/> (or written to the standard error when there is none) and the schedule continues.
/// Override <see cref="OnExecutionFailedAsync(Exception, CancellationToken)"/> to handle them.</para>
/// </remarks>
public abstract class MilvaCronJobService : IHostedService
{
    private Task _executingTask;
    private CancellationTokenSource _cts;
    private readonly IScheduleConfig _scheduleConfig;
    private readonly CronExpression _expression;
    private readonly TimeZoneInfo _timeZone;

    /// <summary>
    /// Logger of job failures. Null when not provided.
    /// </summary>
    protected ILogger Logger { get; }

    /// <summary>
    /// Initializes new instances of <see cref="MilvaCronJobService"/>
    /// </summary>
    /// <param name="scheduleConfig"></param>
    protected MilvaCronJobService(IScheduleConfig scheduleConfig) : this(scheduleConfig, logger: null)
    {
    }

    /// <summary>
    /// Initializes new instances of <see cref="MilvaCronJobService"/>
    /// </summary>
    /// <param name="scheduleConfig"></param>
    /// <param name="logger">Logger of job failures.</param>
    protected MilvaCronJobService(IScheduleConfig scheduleConfig, ILogger logger)
    {
        ArgumentNullException.ThrowIfNull(scheduleConfig);

        _scheduleConfig = scheduleConfig;
        _expression = CronExpression.Parse(scheduleConfig.CronExpression, scheduleConfig.CronFormat);
        _timeZone = scheduleConfig.TimeZoneInfo ?? (scheduleConfig.UseUtcDateTimes ? TimeZoneInfo.Utc : TimeZoneInfo.Local);
        Logger = logger;
    }

    /// <inheritdoc/>
    public Task StartAsync(CancellationToken cancellationToken)
    {
        _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

        _executingTask = ScheduleAndRunAsync(_cts.Token);

        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public async Task StopAsync(CancellationToken cancellationToken)
    {
        if (_executingTask == null || _cts == null || _cts.IsCancellationRequested)
            return;

        try
        {
            if (!_cts.IsCancellationRequested)
                await _cts.CancelAsync();

            await Task.WhenAny(_executingTask, Task.Delay(-1, cancellationToken));
        }
        catch (ObjectDisposedException)
        {
            // Already disposed, ignore
        }
        finally
        {
            try
            {
                _cts.Dispose();
            }
            catch (ObjectDisposedException)
            {
                // Already disposed, ignore
            }
        }
    }

    /// <summary>
    /// Executes the job.
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public abstract Task ExecuteAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Called when <see cref="ExecuteAsync(CancellationToken)"/> throws. The schedule continues afterwards.
    /// </summary>
    /// <param name="exception"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    protected virtual Task OnExecutionFailedAsync(Exception exception, CancellationToken cancellationToken)
    {
        if (Logger != null)
            Logger.LogError(exception, "Scheduled job {JobName} failed.", _scheduleConfig.Name);
        else
            Console.Error.WriteLine($"{_scheduleConfig.Name} failed! {exception}");

        return Task.CompletedTask;
    }

    /// <summary>
    /// Gets the current time used for scheduling.
    /// </summary>
    /// <returns></returns>
    protected virtual DateTimeOffset GetNow() => CommonHelper.GetDateTimeOffsetNow(_scheduleConfig.UseUtcDateTimes);

    /// <summary>
    /// Gets the next occurrence after <paramref name="now"/> and strictly after <paramref name="previousOccurrence"/>.
    /// </summary>
    /// <param name="now"></param>
    /// <param name="previousOccurrence">The occurrence that already ran, if any.</param>
    /// <returns></returns>
    protected DateTimeOffset? GetNextOccurrence(DateTimeOffset now, DateTimeOffset? previousOccurrence)
    {
        var from = previousOccurrence is DateTimeOffset previous && previous > now ? previous : now;

        return _expression.GetNextOccurrence(from, _timeZone);
    }

    /// <summary>
    /// Schedules and runs the job.
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    protected virtual async Task ScheduleAndRunAsync(CancellationToken cancellationToken)
    {
        DateTimeOffset? previousOccurrence = null;

        while (!cancellationToken.IsCancellationRequested)
        {
            var now = GetNow();
            var next = GetNextOccurrence(now, previousOccurrence);

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

                delay = next.Value - GetNow();
            }

            if (delay > TimeSpan.Zero)
                await Task.Delay(delay, cancellationToken);

            previousOccurrence = next;

            if (cancellationToken.IsCancellationRequested)
                break;

            try
            {
                await ExecuteAsync(cancellationToken);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                await OnExecutionFailedAsync(ex, cancellationToken);
            }
        }
    }
}
