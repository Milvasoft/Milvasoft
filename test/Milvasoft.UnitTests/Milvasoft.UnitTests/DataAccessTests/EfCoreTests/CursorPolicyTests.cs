using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Milvasoft.Components.Rest.Enums;
using Milvasoft.Components.Rest.Request;
using Milvasoft.Core.Exceptions;
using Milvasoft.DataAccess.EfCore.Configuration;
using Milvasoft.DataAccess.EfCore.Utils;
using Milvasoft.UnitTests.ComponentsTests.RestTests.RequestTests;
using Milvasoft.UnitTests.DataAccessTests.EfCoreTests.Fixtures;
using System.Text;
using System.Text.Json;

namespace Milvasoft.UnitTests.DataAccessTests.EfCoreTests;

/// <summary>
/// Cursor paging validates the sort property of the request and of the (client supplied, unsigned) cursor with <see cref="RequestPropertyPolicy"/>.
/// </summary>
[Trait("EF Core Data Access Unit Tests", "Unit tests for Milvasoft.DataAccess.EfCore unit testable parts.")]
public class CursorPolicyTests
{
    [Theory]
    [InlineData(nameof(PolicyTestEntityFixture.PasswordHash))]
    [InlineData(nameof(PolicyTestEntityFixture.EncryptedValue))]
    [InlineData("NotExists")]
    public async Task ToCursorListResponseAsync_WithForgedCursorSortProperty_ShouldThrow(string sortBy)
    {
        // Arrange
        var dbContext = await CursorPolicyTestHelper.CreateContextAsync();

        var request = new CursorListRequest { RowCount = 2, Cursor = CursorPolicyTestHelper.ForgeCursor(sortBy, "x") };

        // Act
        var act = () => dbContext.PolicyTestEntities.ToCursorListResponseAsync(request, TestContext.Current.CancellationToken);
        var projectedAct = () => dbContext.PolicyTestEntities.ToCursorListResponseAsync(request, e => new PolicyTestEntityFixture { Id = e.Id }, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        await act.Should().ThrowAsync<MilvaUserFriendlyException>();
        await projectedAct.Should().ThrowAsync<MilvaUserFriendlyException>();
    }

    [Fact]
    public async Task ToCursorListResponseAsync_WithSensitiveRequestedSortProperty_ShouldThrow()
    {
        // Arrange
        var dbContext = await CursorPolicyTestHelper.CreateContextAsync();

        var request = new CursorListRequest { RowCount = 2, Sorting = new SortRequest { SortBy = nameof(PolicyTestEntityFixture.PasswordHash) } };

        // Act
        var act = () => dbContext.PolicyTestEntities.ToCursorListResponseAsync(request, TestContext.Current.CancellationToken);

        // Assert
        await act.Should().ThrowAsync<MilvaUserFriendlyException>();
    }
}

/// <summary>
/// Changes <see cref="RequestQueryOptions.RequireExplicitOptIn"/>, so it runs in the not parallel <see cref="RequestQueryOptionsCollection"/>.
/// </summary>
[Collection(nameof(RequestQueryOptionsCollection))]
[Trait("EF Core Data Access Unit Tests", "Unit tests for Milvasoft.DataAccess.EfCore unit testable parts.")]
public class CursorPolicyStrictModeTests
{
    [Fact]
    public async Task ToCursorListResponseAsync_InStrictMode_ShouldPageByExplicitlyAllowedPropertyWithIdTieBreak()
    {
        // Arrange
        var dbContext = await CursorPolicyTestHelper.CreateContextAsync();

        RequestQueryOptions.RequireExplicitOptIn = true;

        try
        {
            var sorting = new SortRequest { SortBy = nameof(PolicyTestEntityFixture.Rank), Type = SortType.Asc };

            // Act
            var firstPage = await dbContext.PolicyTestEntities.ToCursorListResponseAsync(new CursorListRequest { RowCount = 2, Sorting = sorting }, TestContext.Current.CancellationToken);
            var secondPage = await dbContext.PolicyTestEntities.ToCursorListResponseAsync(new CursorListRequest { RowCount = 2, Cursor = firstPage.NextCursor }, TestContext.Current.CancellationToken);

            var defaultFirstPage = await dbContext.PolicyTestEntities.ToCursorListResponseAsync(new CursorListRequest { RowCount = 2 }, TestContext.Current.CancellationToken);
            var defaultSecondPage = await dbContext.PolicyTestEntities.ToCursorListResponseAsync(new CursorListRequest { RowCount = 2, Cursor = defaultFirstPage.NextCursor }, TestContext.Current.CancellationToken);

            // Assert
            firstPage.Data.Select(e => e.Id).Should().Equal(1, 2);
            secondPage.Data.Select(e => e.Id).Should().Equal(3, 4);
            defaultFirstPage.Data.Select(e => e.Id).Should().Equal(5, 4);
            defaultSecondPage.Data.Select(e => e.Id).Should().Equal(3, 2);
        }
        finally
        {
            RequestQueryOptions.RequireExplicitOptIn = false;
        }
    }

    [Fact]
    public async Task ToCursorListResponseAsync_InStrictMode_WithNotExplicitlyAllowedSortProperty_ShouldThrow()
    {
        // Arrange
        var dbContext = await CursorPolicyTestHelper.CreateContextAsync();

        RequestQueryOptions.RequireExplicitOptIn = true;

        try
        {
            var request = new CursorListRequest { RowCount = 2, Cursor = CursorPolicyTestHelper.ForgeCursor(nameof(PolicyTestEntityFixture.Name), "name1") };

            // Act
            var act = () => dbContext.PolicyTestEntities.ToCursorListResponseAsync(request, TestContext.Current.CancellationToken);

            // Assert
            await act.Should().ThrowAsync<MilvaUserFriendlyException>();
        }
        finally
        {
            RequestQueryOptions.RequireExplicitOptIn = false;
        }
    }
}

internal static class CursorPolicyTestHelper
{
    /// <summary>
    /// Builds a cursor like a client could (cursors are base64 JSON and not signed).
    /// </summary>
    public static string ForgeCursor(string sortBy, string value)
    {
        var json = JsonSerializer.Serialize(new { SortBy = sortBy, Value = JsonSerializer.Serialize(value), SortType = SortType.Asc, IsBackward = false });

        return Convert.ToBase64String(Encoding.UTF8.GetBytes(json));
    }

    public static async Task<SomeMilvaDbContextFixture> CreateContextAsync()
    {
        var services = new ServiceCollection();

        services.AddSingleton<IDataAccessConfiguration>(new DataAccessConfiguration());
        services.AddDbContext<SomeMilvaDbContextFixture>(opt => opt.UseInMemoryDatabase($"CursorPolicyTests_{Guid.NewGuid()}"));

        var dbContext = services.BuildServiceProvider().GetRequiredService<SomeMilvaDbContextFixture>();

        for (int i = 1; i <= 5; i++)
            dbContext.PolicyTestEntities.Add(new PolicyTestEntityFixture { Id = i, Name = $"name{i}", PasswordHash = $"hash{i}", Rank = i * 10 });

        await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        dbContext.ChangeTracker.Clear();

        return dbContext;
    }
}
