using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Milvasoft.BulkExtensions.Tests.Infrastructure;
using System.Collections.Concurrent;
using System.Transactions;

namespace Milvasoft.BulkExtensions.Tests;

/// <summary>
/// Transactions, cancellation, concurrency, logging and the synchronous API.
/// </summary>
public class ExecutionTests(PostgresFixture fixture) : BulkTestBase(fixture)
{
    [Fact]
    public async Task AmbientTransactionScope_RollbackDiscardsRows()
    {
        using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        {
            await using var context = CreateContext();

            await context.BulkInsertAsync([NewProduct("A")], c => c.SetOutputIdentity = true, CancellationToken);
            await context.BulkInsertOrUpdateAsync([NewProduct("B")], c => c.UpdateByProperties = [nameof(Product.Sku)], CancellationToken);

            // Not completed: rolled back on dispose.
        }

        await using var verify = CreateContext();

        (await verify.Products.CountAsync(CancellationToken)).Should().Be(0);
    }

    [Fact]
    public async Task AmbientTransactionScope_CompleteCommitsRows()
    {
        using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        {
            await using var context = CreateContext();

            await context.BulkInsertAsync([NewProduct("A"), NewProduct("B")], cancellationToken: CancellationToken);
            await context.BulkDeleteAsync([new Product { Sku = "B" }], c => c.UpdateByProperties = [nameof(Product.Sku)], CancellationToken);

            scope.Complete();
        }

        await using var verify = CreateContext();

        (await verify.Products.Select(p => p.Sku).ToListAsync(CancellationToken)).Should().Equal("A");
    }

    [Fact]
    public async Task UserTransaction_CommitKeepsEveryOperation()
    {
        await using (var context = CreateContext())
        {
            await using var transaction = await context.Database.BeginTransactionAsync(CancellationToken);

            var products = new[] { NewProduct("A"), NewProduct("B"), NewProduct("C") };

            await context.BulkInsertAsync(products, c => c.SetOutputIdentity = true, CancellationToken);
            await context.BulkUpdateAsync([products[0]], c => c.PropertiesToInclude = [nameof(Product.Price)], CancellationToken);
            await context.BulkDeleteAsync([products[2]], cancellationToken: CancellationToken);

            context.Add(NewProduct("D"));
            await context.BulkSaveChangesAsync(cancellationToken: CancellationToken);

            await transaction.CommitAsync(CancellationToken);
        }

        await using var verify = CreateContext();

        (await verify.Products.OrderBy(p => p.Sku).Select(p => p.Sku).ToListAsync(CancellationToken)).Should().Equal("A", "B", "D");
    }

    [Fact]
    public async Task FailureInsideUserTransaction_LeavesItForTheCallerToRollBack()
    {
        await SeedAsync(NewProduct("taken"));

        await using var context = CreateContext();
        await using var transaction = await context.Database.BeginTransactionAsync(CancellationToken);

        var product = NewProduct("taken");
        var act = () => context.BulkInsertAsync([product], c => c.SetOutputIdentity = true, CancellationToken);

        await act.Should().ThrowAsync<Npgsql.PostgresException>();
        product.Id.Should().Be(0, "values are not written back after a failure");

        await transaction.RollbackAsync(CancellationToken);

        (await context.BulkInsertAsync([NewProduct("free")], cancellationToken: CancellationToken)).Should().Be(1, "the context is usable after the rollback");
    }

    [Fact]
    public async Task CanceledToken_StopsTheOperation()
    {
        await using var context = CreateContext();

        using var cancellation = new CancellationTokenSource();
        await cancellation.CancelAsync();

        var act = () => context.BulkInsertAsync([NewProduct("A")], cancellationToken: cancellation.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
        (await context.Products.CountAsync(CancellationToken)).Should().Be(0);
    }

    [Fact]
    public async Task ParallelOperationsOnSeparateContexts_DoNotInterfere()
    {
        var tasks = Enumerable.Range(0, 8).Select(async worker =>
        {
            await using var context = CreateContext();

            var products = Enumerable.Range(0, 500).Select(i => NewProduct($"w{worker}-{i}", i)).ToList();

            await context.BulkInsertAsync(products, c => c.SetOutputIdentity = true, CancellationToken);
            products.ForEach(p => p.Price += 1000);
            await context.BulkUpdateAsync(products, c => c.PropertiesToInclude = [nameof(Product.Price)], CancellationToken);
            await context.BulkInsertOrUpdateAsync(products.Take(100), c => c.UpdateByProperties = [nameof(Product.Sku)], CancellationToken);

            return products;
        });

        var results = await Task.WhenAll(tasks);

        results.SelectMany(r => r.Select(p => p.Id)).Should().OnlyHaveUniqueItems();

        await using var verify = CreateContext();

        (await verify.Products.CountAsync(CancellationToken)).Should().Be(4000);
        (await verify.Products.CountAsync(p => p.Price < 1000, CancellationToken)).Should().Be(0);
    }

    [Fact]
    public async Task CommandTimeout_IsAppliedToStatements()
    {
        await SeedAsync(NewProduct("A"));

        await using var context = CreateContext();
        await using var transaction = await context.Database.BeginTransactionAsync(CancellationToken);

        // Hold a row lock in another session so the update has to wait longer than the timeout.
        await using var blocker = CreateContext();
        await using var blockerTransaction = await blocker.Database.BeginTransactionAsync(CancellationToken);
        await blocker.Database.ExecuteSqlRawAsync("SELECT 1 FROM \"Products\" FOR UPDATE", CancellationToken);

        var seeded = await context.Products.AsNoTracking().SingleAsync(CancellationToken);
        var act = () => context.BulkUpdateAsync([seeded], c => c.CommandTimeout = 1, CancellationToken);

        var started = DateTime.UtcNow;

        await act.Should().ThrowAsync<Exception>();
        (DateTime.UtcNow - started).Should().BeLessThan(TimeSpan.FromSeconds(20));
    }

    [Fact]
    public async Task Statements_AreLoggedAtDebugLevel()
    {
        var provider = new CollectingLoggerProvider();
        using var loggerFactory = LoggerFactory.Create(b => b.AddProvider(provider).SetMinimumLevel(LogLevel.Debug));

        await using (var context = CreateContext(configureOptions: o => o.UseLoggerFactory(loggerFactory)))
            await context.BulkInsertOrUpdateAsync([NewProduct("A")], c => c.UpdateByProperties = [nameof(Product.Sku)], CancellationToken);

        provider.Messages.Should().Contain(m => m.Contains("COPY") && m.Contains("FROM STDIN"));
        provider.Messages.Should().Contain(m => m.Contains("ON CONFLICT"));
    }

    [Fact]
    public async Task SynchronousApi_CoversEveryOperation()
    {
        await using var context = CreateContext();

        var products = new List<Product> { NewProduct("A", 1), NewProduct("B", 2) };

        context.BulkInsert(products, c => c.SetOutputIdentity = true).Should().Be(2);
        products[0].Price = 10;
        context.BulkUpdate(products, new BulkConfig { PropertiesToInclude = [nameof(Product.Price)] }).Should().Be(2);
        context.BulkInsertOrUpdate([NewProduct("C", 3)], c => c.UpdateByProperties = [nameof(Product.Sku)]).Should().Be(1);
        context.BulkRead([new Product { Id = products[0].Id }]).Single().Price.Should().Be(10);
        context.BulkRead([new Product { Sku = "C" }], c => c.UpdateByProperties = [nameof(Product.Sku)]).Should().ContainSingle();
        context.BulkDelete([products[1]], c => c.PropertiesToInclude = null).Should().Be(1);

        context.ChangeTracker.Clear();
        context.Add(NewProduct("D", 4));
        context.BulkSaveChanges(new BulkConfig { CommandTimeout = 30 }).Should().Be(1);

        context.Truncate<Inventory>();

        (await context.Products.AsNoTracking().OrderBy(p => p.Sku).Select(p => p.Sku).ToListAsync(CancellationToken)).Should().Equal("A", "C", "D");
    }

    [Fact]
    public async Task SynchronousApi_WithRetryingStrategy()
    {
        await using var context = CreateContext(configure: o => o.EnableRetryOnFailure());

        context.BulkInsert([NewProduct("A")]).Should().Be(1);

        context.Add(new TreeNode { Name = "root", Children = [new TreeNode { Name = "child" }] });
        context.BulkSaveChanges().Should().Be(2);

        (await context.TreeNodes.CountAsync(CancellationToken)).Should().Be(2);
    }

    [Fact]
    public async Task SameEntitiesInsertedTwice_SecondCallFailsOnKeyNotSilently()
    {
        await using var context = CreateContext();

        var inventory = new[] { new Inventory { WarehouseId = 1, ProductCode = "X" } };

        await context.BulkInsertAsync(inventory, cancellationToken: CancellationToken);

        var act = () => context.BulkInsertAsync(inventory, cancellationToken: CancellationToken);

        await act.Should().ThrowAsync<Npgsql.PostgresException>().Where(e => e.SqlState == "23505");
    }

    private sealed class CollectingLoggerProvider : ILoggerProvider
    {
        public ConcurrentQueue<string> Messages { get; } = new();

        public ILogger CreateLogger(string categoryName) => new Collector(this, categoryName);

        public void Dispose()
        {
        }

        private sealed class Collector(CollectingLoggerProvider provider, string category) : ILogger
        {
            public IDisposable BeginScope<TState>(TState state) where TState : notnull => null;

            public bool IsEnabled(LogLevel logLevel) => category == "Milvasoft.BulkExtensions";

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
            {
                if (IsEnabled(logLevel))
                    provider.Messages.Enqueue(formatter(state, exception));
            }
        }
    }
}