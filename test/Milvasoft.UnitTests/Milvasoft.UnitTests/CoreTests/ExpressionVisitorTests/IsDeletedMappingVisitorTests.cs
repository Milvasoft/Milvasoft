using FluentAssertions;
using Milvasoft.Core.EntityBases.Abstract.Auditing;
using Milvasoft.Core.EntityBases.Concrete.Auditing;
using Milvasoft.Core.Utils.ExpressionVisitors;
using System.Linq.Expressions;

namespace Milvasoft.UnitTests.CoreTests.ExpressionVisitorTests;

[Trait("Core Unit Tests", "Milvasoft.Core project unit tests.")]
public class IsDeletedMappingVisitorTests
{
    public class SoftDeleteTestEntity : FullAuditableEntity<int>
    {
        public string Name { get; set; }
        public List<SoftDeleteTestEntity> Children { get; set; }
    }

    [Fact]
    public void VisitMemberInit_ShouldAddIsDeletedProperty_WhenNotMapped()
    {
        // Arrange
        var visitor = new IsDeletedMappingVisitor();

        Expression<Func<SoftDeleteTestEntity, SoftDeleteTestEntity>> expression = s => new SoftDeleteTestEntity
        {
            Name = s.Name
        };

        var memberInitExpression = expression.Body as MemberInitExpression;

        // Act
        var result = visitor.Visit(memberInitExpression) as MemberInitExpression;

        // Assert
        result.Should().NotBeNull();
        result.Bindings.Should().HaveCount(2);
        result.Bindings.Should().Contain(binding => binding.BindingType == MemberBindingType.Assignment && binding.Member.Name == nameof(ISoftDeletable.IsDeleted));
    }

    [Fact]
    public void VisitMemberInit_ShouldNotAddIsDeletedProperty_WhenAlreadyMapped()
    {
        // Arrange
        var visitor = new IsDeletedMappingVisitor();

        Expression<Func<SoftDeleteTestEntity, SoftDeleteTestEntity>> expression = s => new SoftDeleteTestEntity
        {
            Name = s.Name,
            IsDeleted = s.IsDeleted
        };

        var memberInitExpression = expression.Body as MemberInitExpression;

        // Act
        var result = visitor.Visit(memberInitExpression) as MemberInitExpression;

        // Assert
        result.Should().NotBeNull();
        result.Bindings.Should().HaveCount(2);
        result.Bindings.Should().Contain(binding => binding.BindingType == MemberBindingType.Assignment && binding.Member.Name == nameof(ISoftDeletable.IsDeleted));
    }
}
