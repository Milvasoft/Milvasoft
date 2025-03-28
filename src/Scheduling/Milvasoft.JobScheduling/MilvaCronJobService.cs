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
public abstract class MilvaCronJobService(IScheduleConfig scheduleConfig) : IHostedService, IDisposable
{
    private bool _disposedValue;
    private System.Timers.Timer _timer;
    private CronExpression _expression = CronExpression.Parse(scheduleConfig.CronExpression, scheduleConfig.CronFormat);
    private TimeZoneInfo _timeZoneInfo = scheduleConfig.TimeZoneInfo;
    private readonly bool _useUtcForDateTimes = scheduleConfig.UseUtcDateTimes;

    /// <summary>
    /// Starts the job.
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public virtual Task StartAsync(CancellationToken cancellationToken) => ScheduleJob(cancellationToken);

    /// <summary>
    /// Schedules the job.
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    protected virtual async Task ScheduleJob(CancellationToken cancellationToken)
    {
        var next = _expression.GetNextOccurrence(CommonHelper.GetDateTimeOffsetNow(_useUtcForDateTimes), _timeZoneInfo);

        if (next.HasValue)
        {
            var delay = next.Value - CommonHelper.GetDateTimeOffsetNow(_useUtcForDateTimes);

            if (delay.TotalMilliseconds <= 0)   // prevent non-positive values from being passed into Timer
                await ScheduleJob(cancellationToken);

            _timer = new System.Timers.Timer(delay.TotalMilliseconds);

            _timer.Elapsed += (sender, args) =>
            {
                _timer.Dispose();
                _timer = null;

                if (!cancellationToken.IsCancellationRequested)
                {
                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            await ExecuteAsync(cancellationToken);

                            if (!cancellationToken.IsCancellationRequested)
                                await ScheduleJob(cancellationToken);
                        }
                        catch (Exception ex)
                        {
                            // Exception'ı burada yakalayabilirsin
                            Console.WriteLine("Timer job execution failed. {0}", ex.Message);
                        }
                    });
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
    public virtual Task ExecuteAsync(CancellationToken cancellationToken) => Task.Delay(5000, cancellationToken);

    /// <summary>
    /// Stops the job.
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public virtual Task StopAsync(CancellationToken cancellationToken)
    {
        _timer?.Stop();
        return Task.CompletedTask;
    }

    /// <summary>
    /// Disposes the timer.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Disposes the timer.
    /// </summary>
    protected virtual void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
                _timer.Dispose();
            }

            _expression = null;
            _timeZoneInfo = null;
            _disposedValue = true;
        }
    }

    /// <summary>
    /// Disposes the timer.
    /// </summary>
    ~MilvaCronJobService() => Dispose(false);
}
