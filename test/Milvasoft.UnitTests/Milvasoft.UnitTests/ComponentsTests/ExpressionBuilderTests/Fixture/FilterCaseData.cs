using ExpressionBuilder.Common;
using ExpressionBuilder.Generics;
using ExpressionBuilder.Operations;
using static Milvasoft.UnitTests.ComponentsTests.ExpressionBuilderTests.Fixture.FilterDataFixture;

namespace Milvasoft.UnitTests.ComponentsTests.ExpressionBuilderTests.Fixture;

/// <summary>
/// Description, filter setup and the expected ids for the entities of <see cref="FilterDataFixture"/>.
/// Covers every operation and type group that <c>FilterRequest</c> allows, plus paths and connectors.
/// </summary>
public class FilterCaseData : TheoryData<string, Action<Filter<FilterEntityFixture>>, int[]>
{
    public FilterCaseData()
    {
        // Text: string. Values and properties are trimmed, matching is case sensitive in memory, null strings never match a value.
        Add("string Contains", f => f.By("Name", new Contains(), "J"), [1, 2]);
        Add("string Contains is case sensitive", f => f.By("Name", new Contains(), "j"), []);
        Add("string Contains trims the value", f => f.By("Name", new Contains(), " oh "), [1]);
        Add("string DoesNotContain", f => f.By("Name", new DoesNotContain(), "J"), [3, 5]);
        Add("string StartsWith", f => f.By("Name", new StartsWith(), "Ja"), [2]);
        Add("string EndsWith", f => f.By("Name", new EndsWith(), "ne"), [2]);
        Add("string EqualTo", f => f.By("Name", new EqualTo(), "Jane"), [2]);
        Add("string EqualTo trims the value", f => f.By("Name", new EqualTo(), "  John "), [1]);
        Add("string EqualTo null", f => f.By("Name", new EqualTo(), (string)null), [4]);
        Add("string EqualTo empty", f => f.By("Name", new EqualTo(), string.Empty), [3, 5]);
        Add("string NotEqualTo", f => f.By("Name", new NotEqualTo(), "John"), [2, 3, 5]);
        Add("string NotEqualTo null", f => f.By("Name", new NotEqualTo(), (string)null), [1, 2, 3, 5]);
        Add("string IsEmpty", f => f.By("Name", new IsEmpty()), [3, 5]);
        Add("string IsNotEmpty", f => f.By("Name", new IsNotEmpty()), [1, 2]);
        Add("string IsNull", f => f.By("Name", new IsNull()), [4]);
        Add("string IsNotNull", f => f.By("Name", new IsNotNull()), [1, 2, 3, 5]);
        Add("string IsNullOrWhiteSpace", f => f.By("Name", new IsNullOrWhiteSpace()), [3, 4, 5]);
        Add("string IsNotNullNorWhiteSpace", f => f.By("Name", new IsNotNullNorWhiteSpace()), [1, 2]);
        Add("string In", f => f.By("Name", new In(), new List<string> { "John", "Mary" }), [1]);
        Add("string In with null item", f => f.By("Name", new In(), new List<string> { "John", null }), [1, 4]);
        Add("string NotIn", f => f.By("Name", new NotIn(), new List<string> { "John", "" }), [2, 4, 5]);

        // Text: char.
        Add("char EqualTo", f => f.By("Code", new EqualTo(), 'A'), [1, 4]);
        Add("char NotEqualTo", f => f.By("Code", new NotEqualTo(), 'A'), [2, 3, 5]);
        Add("char In", f => f.By("Code", new In(), new List<char> { 'B', 'C' }), [2, 3]);
        Add("char NotIn", f => f.By("Code", new NotIn(), new[] { 'A', 'D' }), [2, 3]);

        // Number: int.
        Add("int EqualTo", f => f.By("Count", new EqualTo(), 5), [2]);
        Add("int NotEqualTo", f => f.By("Count", new NotEqualTo(), 5), [1, 3, 4, 5]);
        Add("int GreaterThan", f => f.By("Count", new GreaterThan(), 10), [4, 5]);
        Add("int GreaterThanOrEqualTo", f => f.By("Count", new GreaterThanOrEqualTo(), 10), [3, 4, 5]);
        Add("int LessThan", f => f.By("Count", new LessThan(), 5), [1]);
        Add("int LessThanOrEqualTo", f => f.By("Count", new LessThanOrEqualTo(), 5), [1, 2]);
        Add("int Between includes both ends", f => f.By("Count", new Between(), 5, 20), [2, 3, 4]);
        Add("int In", f => f.By("Count", new In(), new List<int> { 1, 10, 99 }), [1, 3]);
        Add("int NotIn", f => f.By("Count", new NotIn(), new List<int> { 1, 10 }), [2, 4, 5]);
        Add("int In with object list", f => f.By("Count", new In(), new List<object> { 1L, "10" }), [1, 3]);

        // Number: int?. Null properties never match a value.
        Add("int? EqualTo", f => f.By("NullableCount", new EqualTo(), 10), [1, 4]);
        Add("int? NotEqualTo skips nulls", f => f.By("NullableCount", new NotEqualTo(), 10), [3]);
        Add("int? GreaterThan", f => f.By("NullableCount", new GreaterThan(), 10), [3]);
        Add("int? GreaterThanOrEqualTo", f => f.By("NullableCount", new GreaterThanOrEqualTo(), 10), [1, 3, 4]);
        Add("int? LessThan", f => f.By("NullableCount", new LessThan(), 30), [1, 4]);
        Add("int? LessThanOrEqualTo", f => f.By("NullableCount", new LessThanOrEqualTo(), 30), [1, 3, 4]);
        Add("int? Between", f => f.By("NullableCount", new Between(), 11, 30), [3]);
        Add("int? EqualTo null", f => f.By("NullableCount", new EqualTo(), (int?)null), [2, 5]);
        Add("int? NotEqualTo null", f => f.By("NullableCount", new NotEqualTo(), (int?)null), [1, 3, 4]);
        Add("int? In with null item", f => f.By("NullableCount", new In(), new List<int?> { 30, null }), [2, 3, 5]);
        Add("int? In with int list", f => f.By("NullableCount", new In(), new List<int> { 30 }), [3]);
        Add("int? NotIn keeps nulls", f => f.By("NullableCount", new NotIn(), new List<int?> { 10 }), [2, 3, 5]);

        // Number: long, decimal, double?. Values are converted to the property type.
        Add("long EqualTo with int value", f => f.By("BigCount", new EqualTo(), 300), [3]);
        Add("long Between", f => f.By("BigCount", new Between(), 200L, 400L), [2, 3, 4]);
        Add("decimal GreaterThan with double value", f => f.By("Price", new GreaterThan(), 20.0), [4, 5]);
        Add("decimal Between", f => f.By("Price", new Between(), 10.5m, 50m), [1, 2, 5]);
        Add("decimal In", f => f.By("Price", new In(), new List<decimal> { 0.5m, 100m }), [3, 4]);
        Add("double? LessThanOrEqualTo", f => f.By("Ratio", new LessThanOrEqualTo(), 0.5), [1, 4]);
        Add("double? EqualTo null", f => f.By("Ratio", new EqualTo(), (double?)null), [2, 5]);

        // Boolean.
        Add("bool EqualTo", f => f.By("IsActive", new EqualTo(), true), [1, 3, 5]);
        Add("bool NotEqualTo", f => f.By("IsActive", new NotEqualTo(), true), [2, 4]);
        Add("bool? EqualTo", f => f.By("IsVerified", new EqualTo(), true), [1, 4]);
        Add("bool? NotEqualTo skips nulls", f => f.By("IsVerified", new NotEqualTo(), true), [2, 5]);
        Add("bool? EqualTo null", f => f.By("IsVerified", new EqualTo(), (bool?)null), [3]);
        Add("bool? NotEqualTo null", f => f.By("IsVerified", new NotEqualTo(), (bool?)null), [1, 2, 4, 5]);

        // Date: DateTime. Values are UTC, so UTC conversion leaves them as they are.
        Add("DateTime Between", f => f.By("CreatedAt", new Between(), Base.AddDays(-3), Base.AddDays(1)), [1, 2, 4]);
        Add("DateTime GreaterThan", f => f.By("CreatedAt", new GreaterThan(), Base), [3, 4]);
        Add("DateTime GreaterThanOrEqualTo", f => f.By("CreatedAt", new GreaterThanOrEqualTo(), Base), [1, 3, 4]);
        Add("DateTime LessThan", f => f.By("CreatedAt", new LessThan(), Base), [2, 5]);
        Add("DateTime LessThanOrEqualTo", f => f.By("CreatedAt", new LessThanOrEqualTo(), Base), [1, 2, 5]);
        Add("DateTime DateEqualTo matches the whole day", f => f.By("CreatedAt", new DateEqualTo(), Base), [1, 4]);
        Add("DateTime GreaterThanOrEqualTo with local value", f => f.By("CreatedAt", new GreaterThanOrEqualTo(), Base.ToLocalTime()), [1, 3, 4]);
        Add("DateTime GreaterThanOrEqualTo with unspecified value is local", f => f.By("CreatedAt", new GreaterThanOrEqualTo(), DateTime.SpecifyKind(Base.ToLocalTime(), DateTimeKind.Unspecified)), [1, 3, 4]);

        // Date: DateTime?.
        Add("DateTime? GreaterThan", f => f.By("UpdatedAt", new GreaterThan(), Base), [1, 3]);
        Add("DateTime? Between", f => f.By("UpdatedAt", new Between(), Base.AddMonths(-2), Base.AddDays(2)), [1, 5]);
        Add("DateTime? LessThan", f => f.By("UpdatedAt", new LessThan(), Base.AddDays(2)), [1, 5]);
        Add("DateTime? DateEqualTo", f => f.By("UpdatedAt", new DateEqualTo(), Base.AddDays(1).AddHours(3)), [1]);
        Add("DateTime? DateEqualTo null", f => f.By("UpdatedAt", new DateEqualTo(), (DateTime?)null), [2, 4]);

        // Date: DateTimeOffset. DateEqualTo uses the day of the value's offset.
        Add("DateTimeOffset Between", f => f.By("PublishedAt", new Between(), Offset(Base.AddDays(-3)), Offset(Base.AddDays(1))), [1, 2, 4]);
        Add("DateTimeOffset GreaterThan", f => f.By("PublishedAt", new GreaterThan(), Offset(Base)), [3, 4]);
        Add("DateTimeOffset LessThan with other offset", f => f.By("PublishedAt", new LessThan(), new DateTimeOffset(2026, 3, 10, 13, 0, 0, TimeSpan.FromHours(3))), [2, 5]);
        Add("DateTimeOffset DateEqualTo", f => f.By("PublishedAt", new DateEqualTo(), new DateTimeOffset(2026, 3, 10, 1, 0, 0, TimeSpan.FromHours(3))), [1, 4]);
        Add("DateTimeOffset DateEqualTo uses the offset's day", f => f.By("PublishedAt", new DateEqualTo(), new DateTimeOffset(2026, 3, 9, 23, 0, 0, TimeSpan.FromHours(-11))), [1]);
        Add("DateTimeOffset GreaterThanOrEqualTo with DateTime value", f => f.By("PublishedAt", new GreaterThanOrEqualTo(), Base), [1, 3, 4]);

        // Date: DateTimeOffset?.
        Add("DateTimeOffset? GreaterThanOrEqualTo", f => f.By("ArchivedAt", new GreaterThanOrEqualTo(), Offset(Base.AddDays(-1))), [2, 3]);
        Add("DateTimeOffset? LessThanOrEqualTo", f => f.By("ArchivedAt", new LessThanOrEqualTo(), Offset(Base)), [2]);
        Add("DateTimeOffset? DateEqualTo null", f => f.By("ArchivedAt", new DateEqualTo(), (DateTimeOffset?)null), [1, 4, 5]);

        // Default: enum, enum?, Guid.
        Add("enum EqualTo", f => f.By("Status", new EqualTo(), FilterStatusFixture.Active), [1, 4]);
        Add("enum EqualTo with int value", f => f.By("Status", new EqualTo(), 2), [3]);
        Add("enum EqualTo with name", f => f.By("Status", new EqualTo(), "passive"), [2, 5]);
        Add("enum NotEqualTo", f => f.By("Status", new NotEqualTo(), FilterStatusFixture.Active), [2, 3, 5]);
        Add("enum In", f => f.By("Status", new In(), new List<FilterStatusFixture> { FilterStatusFixture.Active, FilterStatusFixture.Deleted }), [1, 3, 4]);
        Add("enum NotIn", f => f.By("Status", new NotIn(), new List<FilterStatusFixture> { FilterStatusFixture.Active }), [2, 3, 5]);
        Add("enum? EqualTo", f => f.By("NullableStatus", new EqualTo(), FilterStatusFixture.Active), [1, 5]);
        Add("enum? NotEqualTo skips nulls", f => f.By("NullableStatus", new NotEqualTo(), FilterStatusFixture.Active), [3, 4]);
        Add("enum? EqualTo null", f => f.By("NullableStatus", new EqualTo(), (FilterStatusFixture?)null), [2]);
        Add("enum? In with null item", f => f.By("NullableStatus", new In(), new List<FilterStatusFixture?> { FilterStatusFixture.Passive, null }), [2, 3]);
        Add("enum? NotIn keeps nulls", f => f.By("NullableStatus", new NotIn(), new List<FilterStatusFixture?> { FilterStatusFixture.Active }), [2, 3, 4]);
        Add("Guid EqualTo", f => f.By("ExternalId", new EqualTo(), Guid3), [3]);
        Add("Guid EqualTo with string value", f => f.By("ExternalId", new EqualTo(), Guid3.ToString()), [3]);
        Add("Guid NotEqualTo", f => f.By("ExternalId", new NotEqualTo(), Guid3), [1, 2, 4, 5]);
        Add("Guid In", f => f.By("ExternalId", new In(), new List<Guid> { Guid2, Guid5 }), [2, 5]);
        Add("Guid NotIn", f => f.By("ExternalId", new NotIn(), new List<Guid> { Guid2, Guid5 }), [1, 3, 4]);

        // Paths.
        Add("reference path skips missing reference", f => f.By("Category.Name", new EqualTo(), "Books"), [1]);
        Add("reference path Contains", f => f.By("Category.Name", new Contains(), "ames"), [4]);
        Add("reference path IsNull matches missing reference", f => f.By("Category.Name", new IsNull()), [2, 3]);
        Add("reference path IsNullOrWhiteSpace matches missing reference", f => f.By("Category.Name", new IsNullOrWhiteSpace()), [2, 3]);
        Add("reference path IsNotNull", f => f.By("Category.Name", new IsNotNull()), [1, 4, 5]);
        Add("property names are case insensitive", f => f.By("category.name", new EqualTo(), "books"), [5]);
        Add("collection path EqualTo", f => f.By("Tags[Name]", new EqualTo(), "blue"), [2, 4]);
        Add("collection path Contains", f => f.By("Tags[Name]", new Contains(), "re"), [1, 2]);
        Add("collection path In", f => f.By("Tags[Name]", new In(), new List<string> { "red" }), [1, 2]);

        // Connectors join a statement with the next one, from left to right.
        Add("no statements matches everything", f => { }, [1, 2, 3, 4, 5]);
        Add("And by default", f => f.By("Count", new GreaterThan(), 1).By("IsActive", new EqualTo(), true), [3, 5]);
        Add("Or", f => f.By("Count", new EqualTo(), 1, Connector.Or).By("Count", new EqualTo(), 50), [1, 5]);
        Add("Or then And", f => f.By("Count", new EqualTo(), 1, Connector.Or).By("Count", new EqualTo(), 5, Connector.And).By("IsActive", new EqualTo(), true), [1]);
        Add("Or with no value operation", f => f.By("Name", new IsNull(), Connector.Or).By("Count", new EqualTo(), 1), [1, 4]);
        Add("Or with two value operation", f => f.By("Count", new Between(), 1, 5, Connector.Or).By("Count", new EqualTo(), 50), [1, 2, 5]);
    }
}
