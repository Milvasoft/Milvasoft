using System.Linq.Expressions;

namespace Milvasoft.Core.Utils.ExpressionVisitors;

/// <summary>
/// Add soft delete check to soft deletable expression.
/// </summary>
public class SoftDeleteFilterVisitor : ExpressionVisitor
{
    private readonly HashSet<Expression> _processedNodes = new HashSet<Expression>();
    private const string _expressionParam = "x";

    /// <inheritdoc/>
    protected override Expression VisitMemberInit(MemberInitExpression node)
    {
        var newBindings = node.Bindings.Select(binding =>
        {
            if (binding is MemberAssignment memberAssignment)
            {
                var expression = Visit(memberAssignment.Expression);
                var propertyType = expression.Type;

                if (IsCollectionOfISoftDeletable(propertyType, out Type elementType))
                {
                    expression = ProcessCollectionExpression(expression, elementType);
                }
                else if (typeof(ISoftDeletable).IsAssignableFrom(propertyType) && !_processedNodes.Contains(expression))
                {
                    expression = ProcessSoftDeletableExpression(expression, propertyType);
                    _processedNodes.Add(expression);
                }

                return Expression.Bind(memberAssignment.Member, expression);
            }
            else
            {
                return binding;
            }
        }).ToList();

        var newExpression = Visit(node.NewExpression) as NewExpression;
        var memberInit = Expression.MemberInit(newExpression, newBindings);

        // Apply the IsDeletedMappingVisitor
        var isDeletedVisitor = new IsDeletedMappingVisitor();

        var newExpression2 = (MemberInitExpression)isDeletedVisitor.Visit(memberInit);

        return newExpression2;
    }

    /// <inheritdoc/>
    protected override Expression VisitMethodCall(MethodCallExpression node)
    {
        var arguments = node.Arguments.Select(Visit).ToList();

        var obj = Visit(node.Object);

        if (node.Method.Name == nameof(Enumerable.Select) || node.Method.Name == nameof(Enumerable.Where))
        {
            return Expression.Call(node.Method, arguments);
        }

        return node.Update(obj, arguments);
    }

    /// <inheritdoc/>
    protected override Expression VisitConditional(ConditionalExpression node)
    {
        var test = Visit(node.Test);
        var ifTrue = Visit(node.IfTrue);
        var ifFalse = Visit(node.IfFalse);

        return Expression.Condition(test, ifTrue, ifFalse);
    }

    private Expression ProcessSoftDeletableExpression(Expression expression, Type propertyType)
    {
        if (_processedNodes.Contains(expression))
            return expression;

        var isDeletedProperty = Expression.Property(expression, nameof(ISoftDeletable.IsDeleted));
        var isDeletedCheck = Expression.Equal(isDeletedProperty, Expression.Constant(false));

        var conditionalExpression = Expression.Condition(
            isDeletedCheck,
            expression,
            Expression.Default(propertyType));

        _processedNodes.Add(expression);
        return conditionalExpression;
    }

    private MethodCallExpression ProcessCollectionExpression(Expression collectionExpression, Type elementType)
    {
        var visitor = new ToListRemoverVisitor();
        var modifiedExpression = visitor.Visit(collectionExpression);

        var parameter = Expression.Parameter(elementType, _expressionParam);

        var isDeletedProperty = Expression.Property(parameter, nameof(ISoftDeletable.IsDeleted));
        var condition = Expression.Equal(isDeletedProperty, Expression.Constant(false));
        var lambda = Expression.Lambda(condition, parameter);

        var whereMethod = typeof(Enumerable).GetMethods()
                                            .First(m => m.Name == nameof(Enumerable.Where) && m.GetParameters().Length == 2)
                                            .MakeGenericMethod(elementType);

        var filteredCollection = Expression.Call(whereMethod, modifiedExpression, lambda);

        var selectLambda = Expression.Lambda(Visit(parameter), parameter);

        var selectMethod = typeof(Enumerable).GetMethods()
                                             .First(m => m.Name == nameof(Enumerable.Select) && m.GetParameters().Length == 2)
                                             .MakeGenericMethod(elementType, selectLambda.Body.Type);

        var processedCollection = Expression.Call(selectMethod, filteredCollection, selectLambda);

        if (collectionExpression.Type.IsGenericType && collectionExpression.Type.GetGenericTypeDefinition() == typeof(List<>))
        {
            var toListMethod = typeof(Enumerable).GetMethods()
                                                 .First(m => m.Name == nameof(Enumerable.ToList) && m.GetParameters().Length == 1)
                                                 .MakeGenericMethod(selectLambda.Body.Type);

            processedCollection = Expression.Call(toListMethod, processedCollection);
        }

        return processedCollection;
    }

    private static bool IsCollectionOfISoftDeletable(Type type, out Type elementType)
    {
        elementType = null;

        if (type.IsGenericType)
        {
            var interfaces = type.GetInterfaces().Concat([type]);

            foreach (var iface in interfaces)
            {
                if (iface.IsGenericType &&
                    iface.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                {
                    var candidateType = iface.GetGenericArguments()[0];

                    if (typeof(ISoftDeletable).IsAssignableFrom(candidateType))
                    {
                        elementType = candidateType;
                        return true;
                    }
                }
            }
        }

        return false;
    }
}