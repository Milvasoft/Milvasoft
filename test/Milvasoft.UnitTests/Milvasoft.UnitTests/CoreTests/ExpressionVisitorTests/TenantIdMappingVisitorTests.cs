using FluentAssertions;
using Milvasoft.Core.EntityBases.Concrete.Auditing;
using Milvasoft.Core.Utils.ExpressionVisitors;
using System.Linq.Expressions;

namespace Milvasoft.UnitTests.CoreTests.ExpressionVisitorTests;

[Trait("Core Unit Tests", "Milvasoft.Core project unit tests.")]
public class TenantIdMappingVisitorTests
{
    public class TenantIdTestEntity : FullAuditableEntityWithTenantId<int>
    {
        public string Name { get; set; }
        public List<TenantIdTestEntity> Children { get; set; }
    }

    [Fact]
    public void VisitMemberInit_ShouldAddTenantIdMapping_IfNotPresent()
    {
        // Arrange
        var parameter = Expression.Parameter(typeof(TenantIdTestEntity), "x");
        var memberInit = Expression.MemberInit(
            Expression.New(typeof(TenantIdTestEntity)),
            Expression.Bind(
                typeof(TenantIdTestEntity).GetProperty(nameof(TenantIdTestEntity.Id)),
                Expression.Property(parameter, nameof(TenantIdTestEntity.Id))
            ),
            Expression.Bind(
                typeof(TenantIdTestEntity).GetProperty(nameof(TenantIdTestEntity.Name)),
                Expression.Property(parameter, nameof(TenantIdTestEntity.Name))
            )
        );

        var visitor = new TenantIdMappingVisitor(parameter);

        // Act
        var result = visitor.Visit(memberInit) as MemberInitExpression;

        // Assert
        result.Should().NotBeNull();
        result.Bindings.Should().Contain(binding => binding.Member.Name == nameof(TenantIdTestEntity.TenantId));
    }

    [Fact]
    public void VisitMemberInit_ShouldNotDuplicateTenantIdMapping_IfAlreadyPresent()
    {
        // Arrange
        var parameter = Expression.Parameter(typeof(TenantIdTestEntity), "x");
        var memberInit = Expression.MemberInit(
            Expression.New(typeof(TenantIdTestEntity)),
            Expression.Bind(
                typeof(TenantIdTestEntity).GetProperty(nameof(TenantIdTestEntity.Id)),
                Expression.Property(parameter, nameof(TenantIdTestEntity.Id))
            ),
            Expression.Bind(
                typeof(TenantIdTestEntity).GetProperty(nameof(TenantIdTestEntity.TenantId)),
                Expression.Property(parameter, nameof(TenantIdTestEntity.TenantId))
            )
        );

        var visitor = new TenantIdMappingVisitor(parameter);

        // Act
        var result = visitor.Visit(memberInit) as MemberInitExpression;

        // Assert
        result.Should().NotBeNull();
        result.Bindings.Count(binding => binding.Member.Name == nameof(TenantIdTestEntity.TenantId)).Should().Be(1);
    }

    [Fact]
    public void VisitMethodCall_ShouldPreserveMethodCallExpression()
    {
        // Arrange
        var parameter = Expression.Parameter(typeof(TenantIdTestEntity), "x");
        var collection = Expression.Constant(new List<TenantIdTestEntity>());
        var whereMethod = typeof(Enumerable).GetMethods()
            .First(m => m.Name == nameof(Enumerable.Where) && m.GetParameters().Length == 2)
            .MakeGenericMethod(typeof(TenantIdTestEntity));

        var lambda = Expression.Lambda(Expression.Constant(true), parameter);
        var methodCall = Expression.Call(whereMethod, collection, lambda);

        var visitor = new TenantIdMappingVisitor(parameter);

        // Act
        var result = visitor.Visit(methodCall) as MethodCallExpression;

        // Assert
        result.Should().NotBeNull();
        result.Method.Name.Should().Be(nameof(Enumerable.Where));
    }

    [Fact]
    public void VisitMemberInit_ShouldHandleNestedChildrenProperly()
    {
        // Arrange
        var parameter = Expression.Parameter(typeof(TenantIdTestEntity), "x");
        var childParameter = Expression.Parameter(typeof(TenantIdTestEntity), "child");

        var childMemberInit = Expression.MemberInit(
            Expression.New(typeof(TenantIdTestEntity)),
            Expression.Bind(
                typeof(TenantIdTestEntity).GetProperty(nameof(TenantIdTestEntity.Name)),
                Expression.Property(childParameter, nameof(TenantIdTestEntity.Name))
            )
        );

        var childrenCollection = Expression.ListInit(
            Expression.New(typeof(List<TenantIdTestEntity>)),
            childMemberInit
        );

        var memberInit = Expression.MemberInit(
            Expression.New(typeof(TenantIdTestEntity)),
            Expression.Bind(
                typeof(TenantIdTestEntity).GetProperty(nameof(TenantIdTestEntity.Name)),
                Expression.Property(parameter, nameof(TenantIdTestEntity.Name))
            ),
            Expression.Bind(
                typeof(TenantIdTestEntity).GetProperty(nameof(TenantIdTestEntity.Children)),
                childrenCollection
            )
        );

        var visitor = new TenantIdMappingVisitor(parameter);

        // Act
        var result = visitor.Visit(memberInit) as MemberInitExpression;

        // Assert
        result.Should().NotBeNull();
        result.Bindings.Should().Contain(binding => binding.Member.Name == nameof(TenantIdTestEntity.TenantId));

        var childrenBinding = result.Bindings
            .FirstOrDefault(binding => binding.Member.Name == nameof(TenantIdTestEntity.Children)) as MemberAssignment;

        childrenBinding.Should().NotBeNull();
        var childrenExpression = childrenBinding.Expression as ListInitExpression;
        childrenExpression.Should().NotBeNull();

        foreach (var childElement in childrenExpression.Initializers)
        {
            var childInit = childElement.Arguments[0] as MemberInitExpression;
            childInit.Should().NotBeNull();
            childInit.Bindings.Should().Contain(binding => binding.Member.Name == nameof(TenantIdTestEntity.TenantId));
        }
    }
}
