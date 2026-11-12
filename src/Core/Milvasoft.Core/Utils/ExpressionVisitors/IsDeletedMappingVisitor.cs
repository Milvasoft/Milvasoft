using System.Linq.Expressions;
using System.Reflection;

namespace Milvasoft.Core.Utils.ExpressionVisitors;

/// <summary>
/// Adds IsDeleted property mapping to the MemberInitExpression.
/// </summary>
/// <remarks>
/// When a projection creates an <see cref="ISoftDeletable"/> type without assigning IsDeleted, the visitor maps it from the source entity of the projection.
/// The source is the instance whose members are assigned to the bindings: a binding that maps a member with the same name (e.g. Id = x.Id) wins,
/// otherwise the instance used by most bindings is chosen. Only sources with a readable IsDeleted property of the same type are considered.
/// </remarks>
public class IsDeletedMappingVisitor : ExpressionVisitor
{
    /// <summary>
    /// Adds IsDeleted property mapping to the MemberInitExpression.
    /// </summary>
    /// <param name="node"></param>
    /// <returns></returns>
    protected override Expression VisitMemberInit(MemberInitExpression node)
    {
        // The base implementation visits nested expressions and keeps every binding kind (assignment, member and list bindings).
        var visited = (MemberInitExpression)base.VisitMemberInit(node);

        if (!typeof(ISoftDeletable).IsAssignableFrom(visited.Type) || visited.Bindings.Any(b => b.Member.Name == nameof(ISoftDeletable.IsDeleted)))
            return visited;

        var isDeletedBinding = CreateIsDeletedBinding(visited);

        if (isDeletedBinding == null)
            return visited;

        return visited.Update(visited.NewExpression, [.. visited.Bindings, isDeletedBinding]);
    }

    private static MemberAssignment CreateIsDeletedBinding(MemberInitExpression node)
    {
        var targetProperty = FindIsDeletedProperty(node.Type);

        if (targetProperty == null || !targetProperty.CanWrite)
            return null;

        var source = FindSourceExpression(node, targetProperty.PropertyType);

        if (source == null)
            return null;

        return Expression.Bind(targetProperty, Expression.Property(source.Expression, source.IsDeletedProperty));
    }

    private static SourceCandidate FindSourceExpression(MemberInitExpression node, Type isDeletedType)
    {
        var candidates = new List<(SourceCandidate Candidate, bool NameMatches)>();

        foreach (var assignment in node.Bindings.OfType<MemberAssignment>())
        {
            var memberExpression = GetAccessedMember(assignment.Expression);

            if (memberExpression?.Expression == null)
                continue;

            var isDeletedProperty = FindIsDeletedProperty(memberExpression.Expression.Type);

            if (isDeletedProperty == null || !isDeletedProperty.CanRead || isDeletedProperty.PropertyType != isDeletedType)
                continue;

            candidates.Add((new SourceCandidate(memberExpression.Expression, isDeletedProperty), memberExpression.Member.Name == assignment.Member.Name));
        }

        if (candidates.Count == 0)
            return null;

        var sameNamed = candidates.FirstOrDefault(c => c.NameMatches);

        if (sameNamed.Candidate != null)
            return sameNamed.Candidate;

        return candidates.GroupBy(c => c.Candidate.Expression.ToString())
                         .OrderByDescending(g => g.Count())
                         .First()
                         .First()
                         .Candidate;
    }

    /// <summary>
    /// Returns the member access of expressions like x.Name, (int?)x.Id or x.Name.Trim().
    /// </summary>
    private static MemberExpression GetAccessedMember(Expression expression) => expression switch
    {
        MemberExpression memberExpression => memberExpression,
        UnaryExpression { NodeType: ExpressionType.Convert or ExpressionType.ConvertChecked } unaryExpression => GetAccessedMember(unaryExpression.Operand),
        MethodCallExpression { Object: MemberExpression objectMember } => objectMember,
        _ => null
    };

    /// <summary>
    /// Finds the most derived IsDeleted property without throwing for hidden (new) properties.
    /// </summary>
    private static PropertyInfo FindIsDeletedProperty(Type type)
    {
        const BindingFlags flags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly;

        for (var current = type; current != null; current = current.BaseType)
        {
            var property = current.GetProperty(nameof(ISoftDeletable.IsDeleted), flags);

            if (property != null)
                return property;
        }

        if (type.IsInterface)
            return type.GetInterfaces().Select(i => i.GetProperty(nameof(ISoftDeletable.IsDeleted), flags)).FirstOrDefault(p => p != null);

        return null;
    }

    private sealed record SourceCandidate(Expression Expression, PropertyInfo IsDeletedProperty);
}
