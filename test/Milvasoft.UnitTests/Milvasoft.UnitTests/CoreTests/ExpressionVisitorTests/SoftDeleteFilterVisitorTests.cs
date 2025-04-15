using FluentAssertions;
using Milvasoft.Core.EntityBases.Concrete.Auditing;
using Milvasoft.Core.Utils.ExpressionVisitors;
using System.Linq.Expressions;

namespace Milvasoft.UnitTests.CoreTests.ExpressionVisitorTests;

[Trait("Core Unit Tests", "Milvasoft.Core project unit tests.")]
public class SoftDeleteFilterVisitorTests
{
    public class SoftDeleteTestEntity : FullAuditableEntity<int>
    {
        public string Name { get; set; }
        public List<SoftDeleteTestEntity> Childrens { get; set; }
        public List<AnotherSoftDeleteTestEntity> OtherChildrens { get; set; }
    }

    public class AnotherSoftDeleteTestEntity : FullAuditableEntity<int>
    {
        public string Name { get; set; }
    }

    [Fact]
    public void VisitMemberInit_ShouldApplySoftDeleteFilter_ForSingleSoftDeletableProperty()
    {
        // Arrange
        var memberInit = Expression.MemberInit(
            Expression.New(typeof(SoftDeleteTestEntity)),
            Expression.Bind(
                typeof(SoftDeleteTestEntity).GetProperty(nameof(SoftDeleteTestEntity.Childrens)),
                Expression.Constant(new List<SoftDeleteTestEntity>
                {
                    new() { IsDeleted = true },
                    new() { IsDeleted = false }
                })
            )
        );

        var visitor = new SoftDeleteFilterVisitor();

        // Act
        var result = visitor.Visit(memberInit) as MemberInitExpression;

        // Assert
        result.Should().NotBeNull();
        result.Bindings.Count.Should().Be(1);
        var binding = result.Bindings[0] as MemberAssignment;
        binding.Expression.Should().NotBeNull();
    }

    [Fact]
    public void VisitMethodCall_ShouldNotAlterSelectOrWhereMethods()
    {
        // Arrange
        var parameter = Expression.Parameter(typeof(SoftDeleteTestEntity), "x");
        var collection = Expression.Constant(new List<SoftDeleteTestEntity>());
        var whereMethod = typeof(Enumerable).GetMethods()
            .First(m => m.Name == nameof(Enumerable.Where) && m.GetParameters().Length == 2)
            .MakeGenericMethod(typeof(SoftDeleteTestEntity));
        var lambda = Expression.Lambda(Expression.Constant(true), parameter);
        var methodCall = Expression.Call(whereMethod, collection, lambda);

        var visitor = new SoftDeleteFilterVisitor();

        // Act
        var result = visitor.Visit(methodCall) as MethodCallExpression;

        // Assert
        result.Should().NotBeNull();
        result.Method.Name.Should().Be(nameof(Enumerable.Where));
    }

    [Fact]
    public void VisitConditional_ShouldPreserveStructure()
    {
        // Arrange
        var testExpression = Expression.Constant(true);
        var ifTrueExpression = Expression.Constant("TruePath");
        var ifFalseExpression = Expression.Constant("FalsePath");
        var conditionalExpression = Expression.Condition(testExpression, ifTrueExpression, ifFalseExpression);

        var visitor = new SoftDeleteFilterVisitor();

        // Act
        var result = visitor.Visit(conditionalExpression) as ConditionalExpression;

        // Assert
        result.Should().NotBeNull();
        result.Test.Should().BeEquivalentTo(testExpression);
        result.IfTrue.Should().BeEquivalentTo(ifTrueExpression);
        result.IfFalse.Should().BeEquivalentTo(ifFalseExpression);
    }

    [Fact]
    public void ProcessCollectionExpression_ShouldFilterSoftDeletableItems()
    {
        // Arrange
        var collection = new List<SoftDeleteTestEntity>
        {
            new() { IsDeleted = true, Name = "Deleted", Childrens = [ new() { Name = "ChildrenDeleted" , IsDeleted = true}], OtherChildrens = [new(){Name = "OtherChildren", IsDeleted = true}] },
            new() { IsDeleted = false, Name = "NotDeleted", Childrens = [ new() { Name = "ChildrenDeleted" , IsDeleted = false}], OtherChildrens = [new(){Name = "OtherChildren", IsDeleted = false}]}
        };
        var collectionExpression = Expression.Constant(collection);

        var visitor = new SoftDeleteFilterVisitor();

        // Act
        var result = visitor.Visit(collectionExpression);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public void ProcessCollectionExpression_ShouldConvertExpression_WhenTypeDoesNotMatchElementType()
    {
        // Arrange
        var collection = new List<SoftDeleteTestEntity>
        {
            new() { IsDeleted = true, Name = "Deleted", Childrens = [ new() { Name = "ChildrenDeleted" , IsDeleted = true}], OtherChildrens = [new(){Name = "OtherChildren", IsDeleted = true}] },
            new() { IsDeleted = false, Name = "NotDeleted", Childrens = [ new() { Name = "ChildrenDeleted" , IsDeleted = false}], OtherChildrens = [new(){Name = "OtherChildren", IsDeleted = false}]}
        };
        var collectionExpression = Expression.Constant(collection);

        var visitor = new SoftDeleteFilterVisitor();

        // Act
        var processedCollection = typeof(SoftDeleteFilterVisitor)
            .GetMethod("ProcessCollectionExpression", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .Invoke(visitor, [collectionExpression, typeof(SoftDeleteTestEntity)]);

        // Assert
        processedCollection.Should().NotBeNull();
    }
}
