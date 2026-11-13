using ExpressionBuilder.Common;
using ExpressionBuilder.Helpers;
using FluentAssertions;
using Milvasoft.UnitTests.ComponentsTests.ExpressionBuilderTests.Fixture;

namespace Milvasoft.UnitTests.ComponentsTests.ExpressionBuilderTests;

[Trait("ExpressionBuilder Unit Tests", "Milvasoft.ExpressionBuilder project unit tests.")]
#pragma warning disable xUnit1045 // Avoid using TheoryData type arguments that might not be serializable
public class OperationHelperTests
{
    public static TheoryData<Type, TypeGroup> Types => new()
    {
        { typeof(string), TypeGroup.Text },
        { typeof(char), TypeGroup.Text },
        { typeof(int), TypeGroup.Number },
        { typeof(int?), TypeGroup.Number },
        { typeof(uint), TypeGroup.Number },
        { typeof(byte), TypeGroup.Number },
        { typeof(sbyte), TypeGroup.Number },
        { typeof(short), TypeGroup.Number },
        { typeof(ushort), TypeGroup.Number },
        { typeof(long), TypeGroup.Number },
        { typeof(ulong), TypeGroup.Number },
        { typeof(float), TypeGroup.Number },
        { typeof(double), TypeGroup.Number },
        { typeof(decimal?), TypeGroup.Number },
        { typeof(bool), TypeGroup.Boolean },
        { typeof(DateTime), TypeGroup.Date },
        { typeof(DateTime?), TypeGroup.Date },
        { typeof(DateTimeOffset), TypeGroup.Date },
        { typeof(Guid), TypeGroup.Default },
        { typeof(FilterStatusFixture), TypeGroup.Default },
        { typeof(DateOnly), TypeGroup.Default },
        { typeof(FilterCategoryFixture), TypeGroup.Default },
    };

    [Theory]
    [MemberData(nameof(Types))]
    public void GetTypeGroup_ShouldReturnGroupOfType(Type type, TypeGroup expected)
        => OperationHelper.GetTypeGroup(type).Should().Be(expected);

    [Theory]
    [MemberData(nameof(Types))]
    public void TypeGroups_ShouldMatchGetTypeGroupByTypeName(Type type, TypeGroup expected)
    {
        // FilterRequest looks types up by name, with the underlying type of nullables.
        var name = (Nullable.GetUnderlyingType(type) ?? type).Name;

        var group = OperationHelper.TypeGroups.FirstOrDefault(i => i.Value.Any(v => v.Name == name));

        (group.Value is null ? TypeGroup.Default : group.Key).Should().Be(expected);
    }
}
#pragma warning restore xUnit1045 // Avoid using TheoryData type arguments that might not be serializable
