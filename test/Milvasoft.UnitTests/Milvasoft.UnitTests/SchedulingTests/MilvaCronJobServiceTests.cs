using Cronos;
using FluentAssertions;
using Milvasoft.JobScheduling;

namespace Milvasoft.UnitTests.SchedulingTests;

[Trait("Scheduling Unit Tests", "Unit tests for Milvasoft.JobScheduling.")]
public class MilvaCronJobServiceTests
{
    [Fact]
    public void NextOccurrence_WithoutTimeZone_ShouldNotThrow()
    {
        // Cronos 0.13 throws ArgumentNullException for a null zone; the service falls back to UTC/local.
        var job = new ProbeJob(new ScheduleConfig { CronExpression = "0 0 * * * *", UseUtcDateTimes = true });

        var next = job.Next(new DateTimeOffset(2026, 1, 1, 10, 30, 0, TimeSpan.Zero), null);

        next.Should().Be(new DateTimeOffset(2026, 1, 1, 11, 0, 0, TimeSpan.Zero));
    }

    [Fact]
    public void NextOccurrence_WhenClockIsBehindThePreviousOccurrence_ShouldNotReturnTheSameOccurrenceAgain()
    {
        var job = new ProbeJob(new ScheduleConfig { CronExpression = "0 0 * * * *", TimeZoneInfo = TimeZoneInfo.Utc });
        var previous = new DateTimeOffset(2026, 1, 1, 11, 0, 0, TimeSpan.Zero);

        // The delay fired a few milliseconds early: "now" is still before the occurrence that just ran.
        var next = job.Next(previous.AddMilliseconds(-5), previous);

        next.Should().Be(previous.AddHours(1));
    }

    [Fact]
    public async Task FailingJob_ShouldReportTheFailureAndKeepRunning()
    {
        var job = new FailingJob(new ScheduleConfig { CronExpression = "* * * * * *", CronFormat = CronFormat.IncludeSeconds, UseUtcDateTimes = true });

        await job.StartAsync(TestContext.Current.CancellationToken);
        await Task.Delay(TimeSpan.FromMilliseconds(2300), TestContext.Current.CancellationToken);
        await job.StopAsync(TestContext.Current.CancellationToken);

        job.Failures.Should().BeGreaterThanOrEqualTo(2);
    }

    private sealed class ProbeJob(IScheduleConfig config) : MilvaCronJobService(config)
    {
        public DateTimeOffset? Next(DateTimeOffset now, DateTimeOffset? previous) => GetNextOccurrence(now, previous);

        public override Task ExecuteAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }

    private sealed class FailingJob(IScheduleConfig config) : MilvaCronJobService(config)
    {
        public int Failures;

        public override Task ExecuteAsync(CancellationToken cancellationToken) => throw new InvalidOperationException("failed");

        protected override Task OnExecutionFailedAsync(Exception exception, CancellationToken cancellationToken)
        {
            Interlocked.Increment(ref Failures);
            return Task.CompletedTask;
        }
    }
}
