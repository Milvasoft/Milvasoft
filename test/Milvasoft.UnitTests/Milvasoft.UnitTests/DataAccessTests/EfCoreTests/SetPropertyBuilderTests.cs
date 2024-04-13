using FluentAssertions;
using Microsoft.EntityFrameworkCore.Query;
using Milvasoft.DataAccess.EfCore.Utils;
using Milvasoft.UnitTests.ComponentsTests.RestTests.Fixture;
using System.Linq.Expressions;

namespace Milvasoft.UnitTests.DataAccessTests.EfCoreTests;

[Trait("EF Core Data Access Unit Tests", "Unit tests for Milvasoft.DataAccess.EfCore unit testable parts.")]
public class SetPropertyBuilderTests
{
    #region SetProperty

    [Fact]
    public void SetProperty_WithPropertyOrValueExpressionIsNull_ShouldReturnDefaultExpression()
    {
        // Arrange
        var builder = new SetPropertyBuilder<RestTestEntityFixture>();
        Expression<Func<SetPropertyCalls<RestTestEntityFixture>, SetPropertyCalls<RestTestEntityFixture>>> expectedExpression = i => i;

        // Act
        var result = builder.SetProperty(null, i => i.Id)
                            .SetProperty(i => i.Id, null)
                            .SetProperty<int>(null, null);

        // Assert
        var equality = ExpressionEqualityComparer.Instance.Equals(expectedExpression, result.SetPropertyCalls);
        equality.Should().BeTrue();
    }

    [Fact]
    public void SetProperty_WithPropertyAndValueExpressionIsValid_ShouldReturnCorrectExpression()
    {
        // Arrange
        var builder = new SetPropertyBuilder<RestTestEntityFixture>();
        Expression<Func<SetPropertyCalls<RestTestEntityFixture>, SetPropertyCalls<RestTestEntityFixture>>> expectedExpression = i => i.SetProperty(i => i.Id, i => i.Id);

        // Act
        var result = builder.SetProperty(i => i.Id, i => i.Id);

        // Assert
        var equality = ExpressionEqualityComparer.Instance.Equals(expectedExpression, result.SetPropertyCalls);
        equality.Should().BeTrue();
    }

    #endregion

    #region SetPropertyValue

    [Fact]
    public void SetPropertyValue_WithPropertyExpressionOrValueIsNull_ShouldReturnDefaultExpression()
    {
        // Arrange
        var builder = new SetPropertyBuilder<RestTestEntityFixture>();
        Expression<Func<SetPropertyCalls<RestTestEntityFixture>, SetPropertyCalls<RestTestEntityFixture>>> expectedExpression = i => i;

        // Act
        var result = builder.SetProperty(null, i => i.UpdateDate)
                            .SetProperty<int>(null, null);

        // Assert
        var equality = ExpressionEqualityComparer.Instance.Equals(expectedExpression, result.SetPropertyCalls);
        equality.Should().BeTrue();
    }

    [Fact]
    public void SetPropertyValue_WithPropertyExpressionAndValueIsValid_ShouldReturnCorrectExpression()
    {
        // Arrange
        var builder = new SetPropertyBuilder<RestTestEntityFixture>();
        Expression<Func<SetPropertyCalls<RestTestEntityFixture>, SetPropertyCalls<RestTestEntityFixture>>> expectedExpression = i => i.SetProperty(i => i.Id, i => 1);

        // Act
        var result = builder.SetProperty(i => i.Id, i => 1);

        // Assert
        var equality = ExpressionEqualityComparer.Instance.Equals(expectedExpression, result.SetPropertyCalls);
        equality.Should().BeTrue();
    }

    [Fact]
    public void SetPropertyValue_WithPropertyExpressionAndValueIsValidButValueIsNull_ShouldReturnCorrectExpression()
    {
        // Arrange
        var builder = new SetPropertyBuilder<RestTestEntityFixture>();
        Expression<Func<SetPropertyCalls<RestTestEntityFixture>, SetPropertyCalls<RestTestEntityFixture>>> expectedExpression = i => i.SetProperty(i => i.UpdateDate, i => null);

        // Act
        var result = builder.SetProperty(i => i.UpdateDate, i => null);

        // Assert
        var equality = ExpressionEqualityComparer.Instance.Equals(expectedExpression, result.SetPropertyCalls);
        equality.Should().BeTrue();
    }

    #endregion
}
