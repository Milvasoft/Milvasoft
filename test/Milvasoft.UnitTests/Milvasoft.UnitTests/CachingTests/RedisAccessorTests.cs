using FluentAssertions;
using Milvasoft.Caching.Redis;
using Milvasoft.Caching.Redis.Options;
using Milvasoft.Core.Exceptions;
using Moq;
using StackExchange.Redis;
using System.Net;

namespace Milvasoft.UnitTests.CachingTests;

[Trait("Caching Unit Tests", "Milvasoft.Caching.Redis project unit tests.")]
public class RedisAccessorTests
{
    private readonly Mock<IConnectionMultiplexer> _multiplexer = new();
    private readonly Mock<IDatabase> _database = new();

    public RedisAccessorTests()
    {
        _database.Setup(d => d.Database).Returns(3);
        _multiplexer.Setup(m => m.GetDatabase(It.IsAny<int>(), It.IsAny<object>())).Returns(_database.Object);
    }

    #region Connection lifecycle

    [Fact]
    public async Task CheckClientAndConnectIfNotAsync_WhenDisconnected_ShouldNotCloseOrReplaceSharedMultiplexer()
    {
        // Arrange
        _multiplexer.Setup(m => m.IsConnected).Returns(false);
        var accessor = new RedisAccessor(_multiplexer.Object);

        // Act
        await accessor.CheckClientAndConnectIfNotAsync();
        await accessor.CheckClientAndConnectIfNotAsync();

        // Assert
        VerifyMultiplexerNotClosed();
        accessor.GetDatabase().Should().BeSameAs(_database.Object);
    }

    [Fact]
    public async Task PerformRedisActionAsync_WhenDisconnected_ShouldThrowUserFriendlyExceptionWithoutRunningActionOrClosingMultiplexer()
    {
        // Arrange
        _multiplexer.Setup(m => m.IsConnected).Returns(false);
        var accessor = new RedisAccessor(_multiplexer.Object);
        var actionRun = false;

        // Act
        Func<Task> act = () => accessor.PerformRedisActionAsync(() =>
        {
            actionRun = true;
            return Task.CompletedTask;
        }, "RedisError");

        // Assert
        await act.Should().ThrowAsync<MilvaUserFriendlyException>().WithMessage("RedisError");
        actionRun.Should().BeFalse();
        VerifyMultiplexerNotClosed();
    }

    [Fact]
    public async Task PerformRedisActionAsync_WhenActionThrowsRedisConnectionException_ShouldWrapItInUserFriendlyException()
    {
        // Arrange
        _multiplexer.Setup(m => m.IsConnected).Returns(true);
        var accessor = new RedisAccessor(_multiplexer.Object);
        var connectionException = new RedisConnectionException(ConnectionFailureType.SocketClosed, CommandFlags.None, "closed", null, CommandStatus.Unknown);

        // Act
        Func<Task<int>> act = () => accessor.PerformRedisActionAsync<int>(() => throw connectionException, "RedisError");

        // Assert
        (await act.Should().ThrowAsync<MilvaUserFriendlyException>()).Which.InnerException.Should().BeSameAs(connectionException);
        VerifyMultiplexerNotClosed();
    }

    [Fact]
    public async Task PerformRedisActionAsync_WhenConnected_ShouldReturnActionResult()
    {
        // Arrange
        _multiplexer.Setup(m => m.IsConnected).Returns(true);
        var accessor = new RedisAccessor(_multiplexer.Object);

        // Act
        var result = await accessor.PerformRedisActionAsync(() => Task.FromResult(42), "RedisError");

        // Assert
        result.Should().Be(42);
    }

    #endregion

    #region Expiration

    [Fact]
    public async Task SetAsync_WithTtl_ShouldPassRelativeDurationUnchanged()
    {
        // Arrange
        Expiration? captured = null;
        _database.Setup(d => d.StringSetAsync(It.IsAny<RedisKey>(), It.IsAny<RedisValue>(), It.IsAny<Expiration>(), It.IsAny<ValueCondition>(), It.IsAny<CommandFlags>()))
                 .Callback<RedisKey, RedisValue, Expiration, ValueCondition, CommandFlags>((_, _, expiration, _, _) => captured = expiration)
                 .ReturnsAsync(true);
        var accessor = new RedisAccessor(_multiplexer.Object);

        // Act
        await accessor.SetAsync("key", "value", TimeSpan.FromMinutes(10));

        // Assert
        captured.Should().Be(new Expiration(TimeSpan.FromMinutes(10)));
    }

    [Fact]
    public void Set_WithTtl_ShouldPassRelativeDurationUnchanged()
    {
        // Arrange
        Expiration? captured = null;
        _database.Setup(d => d.StringSet(It.IsAny<RedisKey>(), It.IsAny<RedisValue>(), It.IsAny<Expiration>(), It.IsAny<ValueCondition>(), It.IsAny<CommandFlags>()))
                 .Callback<RedisKey, RedisValue, Expiration, ValueCondition, CommandFlags>((_, _, expiration, _, _) => captured = expiration)
                 .Returns(true);
        var accessor = new RedisAccessor(_multiplexer.Object);

        // Act
        accessor.Set("key", "value", TimeSpan.FromMinutes(10));

        // Assert
        captured.Should().Be(new Expiration(TimeSpan.FromMinutes(10)));
    }

    [Fact]
    public void Set_WithNullExpiration_ShouldSetWithoutExpiration()
    {
        // Arrange
        var accessor = new RedisAccessor(_multiplexer.Object);

        // Act
        Action act = () => accessor.Set("key", "value", null);

        // Assert
        act.Should().NotThrow();
        _database.Invocations.Should().ContainSingle(i => i.Method.Name == nameof(IDatabase.StringSet));
        _database.Invocations.SelectMany(i => i.Arguments).OfType<Expiration>().Should().OnlyContain(e => e.Equals(default(Expiration)));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task SetAsync_WithNonPositiveTtl_ShouldThrowArgumentOutOfRangeException(int seconds)
    {
        // Arrange
        var accessor = new RedisAccessor(_multiplexer.Object);

        // Act
        Func<Task> act = () => accessor.SetAsync("key", "value", TimeSpan.FromSeconds(seconds));

        // Assert
        await act.Should().ThrowAsync<ArgumentOutOfRangeException>();
    }

    [Fact]
    public async Task KeyExpireAsync_WithTtl_ShouldPassRelativeDurationUnchanged()
    {
        // Arrange
        var accessor = new RedisAccessor(_multiplexer.Object);

        // Act
        await accessor.KeyExpireAsync("key", TimeSpan.FromMinutes(10));

        // Assert
        _database.Verify(d => d.KeyExpireAsync(It.IsAny<RedisKey>(), (TimeSpan?)TimeSpan.FromMinutes(10), It.IsAny<ExpireWhen>(), It.IsAny<CommandFlags>()), Times.Once);
    }

    [Fact]
    public void KeyExpire_WithUnspecifiedKindDate_ShouldThrowArgumentException()
    {
        // Arrange
        var accessor = new RedisAccessor(_multiplexer.Object);

        // Act
        Action act = () => accessor.KeyExpire("key", new DateTime(2030, 1, 1, 0, 0, 0, DateTimeKind.Unspecified));

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    #endregion

    #region Purge

    [Fact]
    public async Task PurgeAsync_ShouldFlushAccessorDatabaseOnConnectedPrimariesOnly()
    {
        // Arrange
        var primary = CreateServer(isReplica: false);
        var replica = CreateServer(isReplica: true);
        _multiplexer.Setup(m => m.GetServers()).Returns([replica.Object, primary.Object]);
        var accessor = new RedisAccessor(_multiplexer.Object);

        // Act
        await accessor.PurgeAsync();
        await accessor.FlushDatabaseAsync();

        // Assert
        primary.Verify(s => s.FlushDatabaseAsync(3, It.IsAny<CommandFlags>()), Times.Exactly(2));
        replica.Verify(s => s.FlushDatabaseAsync(It.IsAny<int>(), It.IsAny<CommandFlags>()), Times.Never);
        VerifyMultiplexerNotClosed();
    }

    [Fact]
    public void Purge_ShouldFlushAccessorDatabaseOnConnectedPrimariesOnly()
    {
        // Arrange
        var primary = CreateServer(isReplica: false);
        var replica = CreateServer(isReplica: true);
        _multiplexer.Setup(m => m.GetServers()).Returns([replica.Object, primary.Object]);
        var accessor = new RedisAccessor(_multiplexer.Object);

        // Act
        accessor.Purge();

        // Assert
        primary.Verify(s => s.FlushDatabase(3, It.IsAny<CommandFlags>()), Times.Once);
        replica.Verify(s => s.FlushDatabase(It.IsAny<int>(), It.IsAny<CommandFlags>()), Times.Never);
    }

    #endregion

    #region Bulk get

    [Fact]
    public async Task GetAsync_WithNoKeys_ShouldReturnEmptyArray()
    {
        // Arrange
        var accessor = new RedisAccessor(_multiplexer.Object);

        // Act
        var nullResult = await accessor.GetAsync((IEnumerable<string>)null);
        var emptyResult = await accessor.GetAsync(Array.Empty<string>());

        // Assert
        nullResult.Should().NotBeNull().And.BeEmpty();
        emptyResult.Should().NotBeNull().And.BeEmpty();
    }

    #endregion

    #region Options

    [Fact]
    public void RedisCachingOptions_ConnectionString_ShouldBeParsedIntoConfigurationOptions()
    {
        // Arrange
        var connectionString = "host1:6379,host2:6380,password=secret,abortConnect=false";

        // Act
        var fromSetter = new RedisCachingOptions { ConnectionString = connectionString };
        var fromCtor = new RedisCachingOptions(connectionString);

        // Assert
        foreach (var options in new[] { fromSetter, fromCtor })
        {
            options.ConfigurationOptions.EndPoints.Should().HaveCount(2);
            options.ConfigurationOptions.EndPoints.OfType<DnsEndPoint>().Select(e => e.Host).Should().BeEquivalentTo("host1", "host2");
            options.ConfigurationOptions.Password.Should().Be("secret");
            options.ConfigurationOptions.AbortOnConnectFail.Should().BeFalse();
        }
    }

    #endregion

    private Mock<IServer> CreateServer(bool isReplica)
    {
        var server = new Mock<IServer>();
        server.Setup(s => s.IsConnected).Returns(true);
        server.Setup(s => s.IsReplica).Returns(isReplica);
        return server;
    }

    private void VerifyMultiplexerNotClosed()
    {
        _multiplexer.Verify(m => m.Close(It.IsAny<bool>()), Times.Never);
        _multiplexer.Verify(m => m.CloseAsync(It.IsAny<bool>()), Times.Never);
        _multiplexer.Verify(m => m.Dispose(), Times.Never);
        _multiplexer.Verify(m => m.DisposeAsync(), Times.Never);
    }
}
