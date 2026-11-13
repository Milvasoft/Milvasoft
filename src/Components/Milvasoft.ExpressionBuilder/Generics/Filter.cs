using ExpressionBuilder.Common;
using ExpressionBuilder.Interfaces;
using ExpressionBuilder.Internal;
using ExpressionBuilder.Operations;
using System.Linq.Expressions;

namespace ExpressionBuilder.Generics;

/// <summary>
/// Builds a predicate for <typeparamref name="TClass"/> from filter statements. The predicate can be translated by EF Core.
/// </summary>
/// <remarks>
/// Each statement is joined with the next one by its connector, from left to right:
/// <c>By(a, Or).By(b, And).By(c)</c> gives <c>(a || b) &amp;&amp; c</c>.
/// <para>
/// Property paths can be <c>Name</c>, <c>Category.Name</c> or <c>Tags[Name]</c>. Statements are checked when they are added,
/// so a wrong property or value throws <see cref="ArgumentException"/> from <c>By</c>.
/// </para>
/// </remarks>
/// <typeparam name="TClass">Filtered type.</typeparam>
public class Filter<TClass> where TClass : class
{
    private readonly ParameterExpression _parameter = Expression.Parameter(typeof(TClass), "x");
    private readonly List<(Expression Predicate, Connector Connector)> _statements = [];

    /// <summary>
    /// Number of statements.
    /// </summary>
    public int Count => _statements.Count;

    /// <summary>
    /// Adds a statement for an operation that takes no value, such as <see cref="IsNull"/>.
    /// </summary>
    /// <param name="propertyId">Property path.</param>
    /// <param name="operation">Operation.</param>
    /// <param name="connector">How this statement is joined with the next one.</param>
    /// <returns>This filter.</returns>
    public Filter<TClass> By(string propertyId, IOperation operation, Connector connector = Connector.And)
        => Add(propertyId, operation, null, null, connector);

    /// <summary>
    /// Adds a statement for an operation that takes one value, such as <see cref="EqualTo"/> or <see cref="In"/>.
    /// </summary>
    /// <param name="propertyId">Property path.</param>
    /// <param name="operation">Operation.</param>
    /// <param name="value">Value. It is converted to the property type when possible.</param>
    /// <param name="connector">How this statement is joined with the next one.</param>
    /// <returns>This filter.</returns>
    public Filter<TClass> By<TValue>(string propertyId, IOperation operation, TValue value, Connector connector = Connector.And)
        => Add(propertyId, operation, value, null, connector);

    /// <summary>
    /// Adds a statement for an operation that takes two values, such as <see cref="Between"/>.
    /// </summary>
    /// <param name="propertyId">Property path.</param>
    /// <param name="operation">Operation.</param>
    /// <param name="value">First value.</param>
    /// <param name="value2">Second value.</param>
    /// <param name="connector">How this statement is joined with the next one.</param>
    /// <returns>This filter.</returns>
    public Filter<TClass> By<TValue>(string propertyId, IOperation operation, TValue value, TValue value2, Connector connector = Connector.And)
        => Add(propertyId, operation, value, value2, connector);

    /// <summary>
    /// Builds the predicate. With no statements it is <c>x => true</c>.
    /// </summary>
    public Expression<Func<TClass, bool>> ToExpression()
    {
        Expression body = null;
        var pendingConnector = Connector.And;

        foreach (var (predicate, connector) in _statements)
        {
            body = body is null
                ? predicate
                : pendingConnector == Connector.And ? Expression.AndAlso(body, predicate) : Expression.OrElse(body, predicate);

            pendingConnector = connector;
        }

        return Expression.Lambda<Func<TClass, bool>>(body ?? Expression.Constant(true), _parameter);
    }

    /// <summary>
    /// Builds the predicate of <paramref name="filter"/>.
    /// </summary>
    public static implicit operator Expression<Func<TClass, bool>>(Filter<TClass> filter) => filter?.ToExpression();

    private Filter<TClass> Add(string propertyId, IOperation operation, object value, object value2, Connector connector)
    {
        ArgumentNullException.ThrowIfNull(operation);

        if (operation is not OperationBase builtIn)
            throw new NotSupportedException($"Operation '{operation.GetType().Name}' is not supported. Use an operation from the {typeof(OperationBase).Namespace} namespace.");

        _statements.Add((PredicateFactory.Create(_parameter, propertyId, builtIn, value, value2), connector));

        return this;
    }
}
