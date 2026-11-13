using ExpressionBuilder.Common;
using ExpressionBuilder.Configuration;
using ExpressionBuilder.Generics;
using ExpressionBuilder.Interfaces;
using ExpressionBuilder.Operations;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Milvasoft.UnitTests.ComponentsTests.ExpressionBuilderTests.Fixture;
using System.Linq.Expressions;
using LessThan = ExpressionBuilder.Operations.LessThan;

namespace Milvasoft.UnitTests.ComponentsTests.ExpressionBuilderTests;

/// <summary>
/// The tests change <see cref="Settings.UseUtcConversionInDateTypes"/>, so they must not run in parallel with other tests.
/// </summary>
[CollectionDefinition(nameof(ExpressionBuilderSettingsCollection), DisableParallelization = true)]
public class ExpressionBuilderSettingsCollection
{
}

[Collection(nameof(ExpressionBuilderSettingsCollection))]
[Trait("ExpressionBuilder Unit Tests", "Milvasoft.ExpressionBuilder project unit tests.")]
#pragma warning disable xUnit1045 // Avoid using TheoryData type arguments that might not be serializable
public class FilterTests : IDisposable
{
    private readonly bool _useUtcConversion = Settings.UseUtcConversionInDateTypes;

    public FilterTests() => Settings.UseUtcConversionInDateTypes = true;

    public void Dispose()
    {
        Settings.UseUtcConversionInDateTypes = _useUtcConversion;
        GC.SuppressFinalize(this);
    }

    #region Semantics

    [Theory]
    [ClassData(typeof(FilterCaseData))]
    public void Filter_ForInMemorySource_ShouldReturnExpectedEntities(string description, Action<Filter<FilterEntityFixture>> setup, int[] expectedIds)
    {
        // Arrange
        var filter = new Filter<FilterEntityFixture>();
        setup(filter);

        // Act
        Expression<Func<FilterEntityFixture, bool>> predicate = filter;
        var result = FilterDataFixture.Create().AsQueryable().Where(predicate).Select(e => e.Id).ToList();

        // Assert
        result.Should().BeEquivalentTo(expectedIds, description);
    }

    [Theory]
    [ClassData(typeof(FilterCaseData))]
    public void Filter_ForNpgsql_ShouldTranslateToSql(string description, Action<Filter<FilterEntityFixture>> setup, int[] expectedIds)
    {
        // Arrange
        _ = expectedIds;
        var filter = new Filter<FilterEntityFixture>();
        setup(filter);
        using var context = FilterNpgsqlDbContextFixture.Create();

        // Act
        var sql = context.Entities.Where(filter.ToExpression()).ToQueryString();

        // Assert
        sql.Should().Contain("FROM \"Entities\"", description);
    }

    [Fact]
    public void Filter_ForNpgsql_ShouldSendValuesAsParameters()
    {
        // Arrange
        var filter = new Filter<FilterEntityFixture>().By("Name", new EqualTo(), "Jane")
                                                      .By("Count", new In(), new List<int> { 7, 8 })
                                                      .By("Status", new NotEqualTo(), FilterStatusFixture.Deleted);
        using var context = FilterNpgsqlDbContextFixture.Create();

        // Act
        var sql = context.Entities.Where(filter.ToExpression()).ToQueryString();
        var query = string.Join('\n', sql.Split('\n').Where(line => !line.StartsWith("--")));

        // Assert
        query.Should().NotContain("Jane").And.Contain("= ANY (");
        sql.Should().Contain("'Jane'");
    }

    [Fact]
    public void Filter_WithUtcConversion_ShouldUseLocalDayBoundsInUtc()
    {
        // Arrange
        var localDay = FilterDataFixture.Base.ToLocalTime().Date;
        var expectedIds = FilterDataFixture.Create().Where(e => e.CreatedAt.ToLocalTime().Date == localDay).Select(e => e.Id);
        var filter = new Filter<FilterEntityFixture>().By("CreatedAt", new DateEqualTo(), DateTime.SpecifyKind(localDay.AddHours(23), DateTimeKind.Local));

        // Act
        var result = FilterDataFixture.Create().AsQueryable().Where(filter.ToExpression()).Select(e => e.Id).ToList();

        // Assert
        result.Should().BeEquivalentTo(expectedIds);
    }

    [Fact]
    public void Filter_WithoutUtcConversion_ShouldUseValuesAsGiven()
    {
        // Arrange
        Settings.UseUtcConversionInDateTypes = false;
        var lateOnBaseDay = new DateTime(2026, 3, 10, 23, 0, 0, DateTimeKind.Utc);
        var unspecifiedBase = DateTime.SpecifyKind(FilterDataFixture.Base, DateTimeKind.Unspecified);
        var dateEqualTo = new Filter<FilterEntityFixture>().By("CreatedAt", new DateEqualTo(), lateOnBaseDay);
        var greaterThanOrEqualTo = new Filter<FilterEntityFixture>().By("CreatedAt", new GreaterThanOrEqualTo(), unspecifiedBase);

        // Act
        var dateEqualToResult = FilterDataFixture.Create().AsQueryable().Where(dateEqualTo.ToExpression()).Select(e => e.Id).ToList();
        var greaterThanOrEqualToResult = FilterDataFixture.Create().AsQueryable().Where(greaterThanOrEqualTo.ToExpression()).Select(e => e.Id).ToList();

        // Assert
        dateEqualToResult.Should().BeEquivalentTo([1, 4]);
        greaterThanOrEqualToResult.Should().BeEquivalentTo([1, 3, 4]);
    }

    [Fact]
    public void Filter_WithMissingReferences_ShouldNotThrow()
    {
        // Arrange
        var data = FilterDataFixture.Create();
        data.ForEach(e => e.Category = null);
        var filter = new Filter<FilterEntityFixture>().By("Category.Name", new StartsWith(), "B");

        // Act
        var result = data.AsQueryable().Where(filter.ToExpression()).ToList();

        // Assert
        result.Should().BeEmpty();
    }

    #endregion

    #region Expression shape

    [Fact]
    public void ToExpression_WithNoStatements_ShouldReturnTruePredicate()
    {
        // Arrange
        Expression<Func<FilterEntityFixture, bool>> expected = x => true;

        // Act
        Expression<Func<FilterEntityFixture, bool>> result = new Filter<FilterEntityFixture>();

        // Assert
        ExpressionEqualityComparer.Instance.Equals(result, expected).Should().BeTrue();
    }

    [Fact]
    public void ImplicitConversion_WithNullFilter_ShouldReturnNull()
    {
        // Arrange
        Filter<FilterEntityFixture> filter = null;

        // Act
        Expression<Func<FilterEntityFixture, bool>> result = filter;

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void By_ShouldReturnSameFilterAndCountStatements()
    {
        // Arrange
        var filter = new Filter<FilterEntityFixture>();

        // Act
        var result = filter.By("Name", new IsNull()).By("Count", new EqualTo(), 1).By("Count", new Between(), 1, 2);

        // Assert
        result.Should().BeSameAs(filter);
        filter.Count.Should().Be(3);
    }

    #endregion

    #region Errors

    public static TheoryData<string, Action<Filter<FilterEntityFixture>>, Type> InvalidStatements => new()
    {
        { "unknown property", f => f.By("Missing", new EqualTo(), 1), typeof(ArgumentException) },
        { "unknown nested property", f => f.By("Category.Missing", new EqualTo(), "x"), typeof(ArgumentException) },
        { "empty path", f => f.By(" ", new IsNull()), typeof(ArgumentException) },
        { "empty path segment", f => f.By("Category..Name", new IsNull()), typeof(ArgumentException) },
        { "unclosed collection path", f => f.By("Tags[Name", new EqualTo(), "red"), typeof(ArgumentException) },
        { "collection path on string", f => f.By("Name[Length]", new EqualTo(), 1), typeof(ArgumentException) },
        { "text operation on number", f => f.By("Count", new Contains(), "1"), typeof(InvalidOperationException) },
        { "text operation on char", f => f.By("Code", new StartsWith(), "A"), typeof(InvalidOperationException) },
        { "DateEqualTo on number", f => f.By("Count", new DateEqualTo(), 1), typeof(InvalidOperationException) },
        { "null value for non nullable property", f => f.By("Count", new EqualTo(), (object)null), typeof(ArgumentException) },
        { "null second value for non nullable property", f => f.By("Count", new Between(), (object)1, null), typeof(ArgumentException) },
        { "value that cannot be converted", f => f.By("Count", new EqualTo(), "abc"), typeof(ArgumentException) },
        { "unknown enum name", f => f.By("Status", new EqualTo(), "Unknown"), typeof(ArgumentException) },
        { "In without list", f => f.By("Count", new In(), 1), typeof(ArgumentException) },
        { "In with string", f => f.By("Name", new In(), "John"), typeof(ArgumentException) },
        { "In with null", f => f.By("Count", new In(), (object)null), typeof(ArgumentException) },
        { "In with null item for non nullable property", f => f.By("Count", new In(), new List<int?> { 1, null }), typeof(ArgumentException) },
        { "NotIn without list", f => f.By("Count", new NotIn(), 1), typeof(ArgumentException) },
        { "operation from another library", f => f.By("Count", new ForeignOperation(), 1), typeof(NotSupportedException) },
        { "null operation", f => f.By("Count", null, 1), typeof(ArgumentNullException) },
    };

    [Theory]
    [MemberData(nameof(InvalidStatements))]
    public void By_WithInvalidStatement_ShouldThrow(string description, Action<Filter<FilterEntityFixture>> setup, Type exceptionType)
    {
        // Arrange
        var filter = new Filter<FilterEntityFixture>();

        // Act
        var exception = Record.Exception(() => setup(filter));

        // Assert
        exception.Should().NotBeNull(description);
        exception.GetType().Should().Be(exceptionType, description);
        filter.Count.Should().Be(0);
    }

    private sealed class ForeignOperation : IOperation
    {
        public string Name => "Foreign";
        public int NumberOfValues => 1;
    }

    #endregion

    #region Operations

    public static TheoryData<IOperation, string, int> Operations => new()
    {
        { new Between(), "Between", 2 },
        { new Contains(), "Contains", 1 },
        { new DateEqualTo(), "DateEqualTo", 1 },
        { new DoesNotContain(), "DoesNotContain", 1 },
        { new EndsWith(), "EndsWith", 1 },
        { new EqualTo(), "EqualTo", 1 },
        { new GreaterThan(), "GreaterThan", 1 },
        { new GreaterThanOrEqualTo(), "GreaterThanOrEqualTo", 1 },
        { new In(), "In", 1 },
        { new IsEmpty(), "IsEmpty", 0 },
        { new IsNotEmpty(), "IsNotEmpty", 0 },
        { new IsNotNull(), "IsNotNull", 0 },
        { new IsNotNullNorWhiteSpace(), "IsNotNullNorWhiteSpace", 0 },
        { new IsNull(), "IsNull", 0 },
        { new IsNullOrWhiteSpace(), "IsNullOrWhiteSpace", 0 },
        { new LessThan(), "LessThan", 1 },
        { new LessThanOrEqualTo(), "LessThanOrEqualTo", 1 },
        { new NotEqualTo(), "NotEqualTo", 1 },
        { new NotIn(), "NotIn", 1 },
        { new StartsWith(), "StartsWith", 1 },
    };

    [Theory]
    [MemberData(nameof(Operations))]
    public void Operation_ShouldHaveNameAndNumberOfValues(IOperation operation, string expectedName, int expectedNumberOfValues)
    {
        // Act & Assert
        operation.Name.Should().Be(expectedName);
        operation.ToString().Should().Be(expectedName);
        operation.NumberOfValues.Should().Be(expectedNumberOfValues);
    }

    [Fact]
    public void Connector_ShouldKeepItsValues()
    {
        // Values are part of the JSON contract of FilterRequest.MergeType.
        ((int)Connector.And).Should().Be(0);
        ((int)Connector.Or).Should().Be(1);
    }

    #endregion
}
#pragma warning restore xUnit1045 // Avoid using TheoryData type arguments that might not be serializable
