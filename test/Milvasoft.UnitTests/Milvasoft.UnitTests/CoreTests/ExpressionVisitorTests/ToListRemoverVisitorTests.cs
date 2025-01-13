using FluentAssertions;
using Milvasoft.Core.EntityBases.Concrete.Auditing;
using Milvasoft.Core.Utils.ExpressionVisitors;
using System.Linq.Expressions;

namespace Milvasoft.UnitTests.CoreTests.ExpressionVisitorTests;

[Trait("Core Unit Tests", "Milvasoft.Core project unit tests.")]
public class ToListRemoverVisitorTests
{
    public class SoftDeleteTestEntity : FullAuditableEntity<int>
    {
        public string Name { get; set; }
    }

    [Fact]
    public void VisitMethodCall_ShouldRemoveToList_ForEnumerableOfISoftDeletable()
    {
        // Arrange
        var sourceData = new List<SoftDeleteTestEntity>
        {
            new() { IsDeleted = false },
            new() { IsDeleted = true }
        };

        Expression<Func<IEnumerable<SoftDeleteTestEntity>>> expression = () => sourceData.Where(x => !x.IsDeleted).ToList();

        var visitor = new ToListRemoverVisitor();

        // Act
        var modifiedExpression = visitor.Visit(expression.Body);

        // Assert
        modifiedExpression.Should().NotBeNull();
        modifiedExpression.ToString().Should().NotContain("ToList()");
    }

    [Fact]
    public void VisitMethodCall_ShouldNotRemoveToList_ForNonISoftDeletableTypes()
    {
        // Arrange
        var sourceData = new List<int> { 1, 2, 3, 4 };

        Expression<Func<IEnumerable<int>>> expression = () => sourceData.Where(x => x > 2).ToList();

        var visitor = new ToListRemoverVisitor();

        // Act
        var modifiedExpression = visitor.Visit(expression.Body);

        // Assert
        modifiedExpression.Should().NotBeNull();
        modifiedExpression.ToString().Should().Contain("ToList()");
    }

    [Fact]
    public void VisitMethodCall_ShouldPreserveOtherMethodCalls()
    {
        // Arrange
        var sourceData = new List<SoftDeleteTestEntity>
        {
            new() { IsDeleted = false },
            new() { IsDeleted = true }
        };

        Expression<Func<IEnumerable<SoftDeleteTestEntity>>> expression = () => sourceData.Where(x => !x.IsDeleted).OrderBy(x => x.IsDeleted).ToList();

        var visitor = new ToListRemoverVisitor();

        // Act
        var modifiedExpression = visitor.Visit(expression.Body);

        // Assert
        modifiedExpression.Should().NotBeNull();
        modifiedExpression.ToString().Should().Contain("OrderBy(x => x.IsDeleted)");
        modifiedExpression.ToString().Should().NotContain("ToList()");
    }

    [Fact]
    public void VisitMethodCall_ShouldHandleNestedToListCalls()
    {
        // Arrange
        var sourceData = new List<SoftDeleteTestEntity>
        {
            new() { IsDeleted = false },
            new() { IsDeleted = true }
        };

        Expression<Func<IEnumerable<SoftDeleteTestEntity>>> expression = () => sourceData.Where(x => !x.IsDeleted).ToList().ToList();

        var visitor = new ToListRemoverVisitor();

        // Act
        var modifiedExpression = visitor.Visit(expression.Body);

        // Assert
        modifiedExpression.Should().NotBeNull();
        modifiedExpression.ToString().Should().NotContain("ToList()");
    }

    [Fact]
    public void VisitMethodCall_ShouldReturnOriginalExpression_IfNoToListPresent()
    {
        // Arrange
        var sourceData = new List<SoftDeleteTestEntity>
        {
            new() { IsDeleted = false },
            new() { IsDeleted = true }
        };

        Expression<Func<IEnumerable<SoftDeleteTestEntity>>> expression = () => sourceData.Where(x => !x.IsDeleted);

        var visitor = new ToListRemoverVisitor();

        // Act
        var modifiedExpression = visitor.Visit(expression.Body);

        // Assert
        modifiedExpression.Should().NotBeNull();
        modifiedExpression.Should().Be(expression.Body);
    }
}
