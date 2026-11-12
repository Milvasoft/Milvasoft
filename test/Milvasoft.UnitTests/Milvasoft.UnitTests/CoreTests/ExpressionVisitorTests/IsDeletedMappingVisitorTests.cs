using FluentAssertions;
using Milvasoft.Core.EntityBases.Abstract.Auditing;
using Milvasoft.Core.EntityBases.Concrete.Auditing;
using Milvasoft.Core.Utils.ExpressionVisitors;
using System.Linq.Expressions;
using static Milvasoft.UnitTests.CoreTests.ExpressionVisitorTests.SoftDeleteFilterVisitorTests;

namespace Milvasoft.UnitTests.CoreTests.ExpressionVisitorTests;

[Trait("Core Unit Tests", "Milvasoft.Core project unit tests.")]
public class IsDeletedMappingVisitorTests
{
    public class SoftDeleteTestEntity : FullAuditableEntity<int>
    {
        public string Name { get; set; }
        public List<SoftDeleteTestEntity> Childrens { get; set; }
        public List<AnotherSoftDeleteTestEntity> OtherChildrens { get; set; }
    }
    public class ParentEntity : FullAuditableEntity<int>
    {
        public string Name { get; set; }
        public AnotherSoftDeleteTestEntity Category { get; set; }
    }

    public class ParentDto : FullAuditableEntity<int>
    {
        public string CategoryName { get; set; }
        public List<string> Tags { get; set; } = [];
        public AnotherSoftDeleteTestEntity Category { get; set; } = new();
    }

    public class MisleadingEntity
    {
        public string Name { get; set; }

        public string IsDeleted { get; set; }
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

    [Fact]
    public void VisitMemberInit_ShouldNotThrow_WhenIsDeletedTypeMismatchOccurs()
    {
        // Arrange
        var visitor = new IsDeletedMappingVisitor();

        // Farklı türde IsDeleted içeren sahte kaynak
        Expression<Func<MisleadingEntity, SoftDeleteTestEntity>> expression = s => new SoftDeleteTestEntity
        {
            Name = s.Name // IsDeleted intentionally not mapped and will be mismatched in source
        };

        var memberInitExpression = expression.Body as MemberInitExpression;

        // Act
        var result = visitor.Visit(memberInitExpression) as MemberInitExpression;

        // Assert
        result.Should().NotBeNull();

        // IsDeleted eklenmemiş olmalı çünkü tip uyuşmazlığı oluşur
        result.Bindings.Should().OnlyContain(b => b.Member.Name != nameof(ISoftDeletable.IsDeleted));
    }

    [Fact]
    public void VisitMemberInit_ShouldMapIsDeletedFromProjectionSource_NotFromNavigation()
    {
        // Arrange
        var visitor = new IsDeletedMappingVisitor();

        Expression<Func<ParentEntity, ParentDto>> expression = p => new ParentDto
        {
            CategoryName = p.Category.Name,
            Id = p.Id
        };

        // Act
        var result = (MemberInitExpression)visitor.Visit(expression.Body);

        // Assert
        var isDeletedBinding = result.Bindings.OfType<MemberAssignment>().Single(b => b.Member.Name == nameof(ISoftDeletable.IsDeleted));
        var isDeletedAccess = (MemberExpression)isDeletedBinding.Expression;
        isDeletedAccess.Expression.Should().BeSameAs(expression.Parameters[0]);
    }

    [Fact]
    public void VisitMemberInit_ShouldKeepListAndMemberBindings()
    {
        // Arrange
        var visitor = new IsDeletedMappingVisitor();

        Expression<Func<ParentEntity, ParentDto>> expression = p => new ParentDto
        {
            Id = p.Id,
            Tags = { "a", "b" },
            Category = { Name = "x" }
        };

        // Act
        var result = (MemberInitExpression)visitor.Visit(expression.Body);

        // Assert
        result.Bindings.Should().Contain(b => b.BindingType == MemberBindingType.ListBinding && b.Member.Name == nameof(ParentDto.Tags));
        result.Bindings.Should().Contain(b => b.BindingType == MemberBindingType.MemberBinding && b.Member.Name == nameof(ParentDto.Category));
        result.Bindings.Should().Contain(b => b.Member.Name == nameof(ISoftDeletable.IsDeleted));

        var dto = Expression.Lambda<Func<ParentEntity, ParentDto>>(result, expression.Parameters).Compile()(new ParentEntity { Id = 3, IsDeleted = true });
        dto.Tags.Should().Equal("a", "b");
        dto.Category.Name.Should().Be("x");
        dto.IsDeleted.Should().BeTrue();
    }
}
