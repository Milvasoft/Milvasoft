using ExpressionBuilder.Operations;
using System.Linq.Expressions;
using System.Reflection;

namespace ExpressionBuilder.Internal;

/// <summary>
/// Builds the predicate of one filter statement from a property path.
/// </summary>
/// <remarks>
/// Supported paths:
/// <list type="bullet">
/// <item><c>Name</c>: a property of the filtered type.</item>
/// <item><c>Category.Name</c>: a property of a reference. Each reference on the way is checked for null.</item>
/// <item><c>Tags[Name]</c>: a property of collection items, matched with <see cref="Enumerable.Any{TSource}(IEnumerable{TSource}, Func{TSource, bool})"/>.</item>
/// </list>
/// Property names are matched case insensitively when there is no exact match.
/// </remarks>
internal static class PredicateFactory
{
    private static readonly MethodInfo _anyMethod = typeof(Enumerable).GetMethods(BindingFlags.Public | BindingFlags.Static)
                                                                      .Single(m => m.Name == nameof(Enumerable.Any) && m.GetParameters().Length == 2);

    public static Expression Create(Expression instance, string propertyPath, OperationBase operation, object value, object value2)
    {
        if (string.IsNullOrWhiteSpace(propertyPath))
            throw new ArgumentException("Property path is required.", nameof(propertyPath));

        var path = propertyPath.Trim();
        var bracketIndex = path.IndexOf('[');

        if (bracketIndex < 0)
        {
            var member = Access(instance, path, out var parents);

            return GuardParents(parents, operation.Build(member, value, value2, path), operation.MatchesMissingParent);
        }

        if (bracketIndex == 0 || path[^1] != ']')
            throw new ArgumentException($"Property path '{propertyPath}' is not valid. Collection paths look like 'Items[Name]'.", nameof(propertyPath));

        var collection = Access(instance, path[..bracketIndex], out var collectionParents);

        var itemType = GetItemType(collection.Type)
                       ?? throw new ArgumentException($"'{path[..bracketIndex]}' is not a collection.", nameof(propertyPath));

        var item = Expression.Parameter(itemType, "i");
        var itemPredicate = Create(item, path[(bracketIndex + 1)..^1], operation, value, value2);
        var any = Expression.Call(_anyMethod.MakeGenericMethod(itemType), collection, Expression.Lambda(itemPredicate, item));

        return GuardParents(collectionParents, any, matchesMissingParent: false);
    }

    /// <summary>
    /// Builds the member access of a dotted path and returns the members on the way in <paramref name="parents"/>.
    /// </summary>
    private static MemberExpression Access(Expression instance, string path, out List<MemberExpression> parents)
    {
        parents = [];

        MemberExpression member = null;

        foreach (var segment in path.Split('.'))
        {
            if (member is not null)
                parents.Add(member);

            var owner = (Expression)member ?? instance;

            member = Expression.Property(owner, FindProperty(owner.Type, segment.Trim(), path));
        }

        return member;
    }

    private static PropertyInfo FindProperty(Type type, string name, string path)
    {
        var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                             .Where(p => p.GetIndexParameters().Length == 0)
                             .ToList();

        var property = Closest(properties.Where(p => p.Name.Equals(name, StringComparison.Ordinal)), type)
                       ?? Closest(properties.Where(p => p.Name.Equals(name, StringComparison.OrdinalIgnoreCase)), type);

        return property ?? throw new ArgumentException($"'{type.Name}' has no public property named '{name}' (path '{path}').", nameof(path));
    }

    /// <summary>
    /// Picks the property declared on the most derived type, so properties hidden with <c>new</c> resolve to the visible one.
    /// </summary>
    private static PropertyInfo Closest(IEnumerable<PropertyInfo> candidates, Type type)
        => candidates.OrderBy(p => Depth(type, p.DeclaringType)).FirstOrDefault();

    private static int Depth(Type type, Type declaringType)
    {
        var depth = 0;

        for (var current = type; current is not null && current != declaringType; current = current.BaseType)
            depth++;

        return depth;
    }

    private static Type GetItemType(Type type)
    {
        if (type == typeof(string))
            return null;

        if (type.IsArray)
            return type.GetElementType();

        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IEnumerable<>))
            return type.GetGenericArguments()[0];

        return type.GetInterfaces()
                   .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                   ?.GetGenericArguments()[0];
    }

    /// <summary>
    /// Adds null checks for the references on the way to the filtered property, innermost first.
    /// </summary>
    /// <param name="parents">References on the way, outermost first.</param>
    /// <param name="predicate">Predicate of the filtered property.</param>
    /// <param name="matchesMissingParent">When true a missing reference satisfies the predicate (<c>IsNull</c>), otherwise it fails it.</param>
    private static Expression GuardParents(List<MemberExpression> parents, Expression predicate, bool matchesMissingParent)
    {
        for (var i = parents.Count - 1; i >= 0; i--)
        {
            var parent = parents[i];

            if (!FilterValues.CanBeNull(parent.Type))
                continue;

            var nullConstant = Expression.Constant(null, parent.Type);

            predicate = matchesMissingParent
                ? Expression.OrElse(Expression.Equal(parent, nullConstant), predicate)
                : Expression.AndAlso(Expression.NotEqual(parent, nullConstant), predicate);
        }

        return predicate;
    }
}
