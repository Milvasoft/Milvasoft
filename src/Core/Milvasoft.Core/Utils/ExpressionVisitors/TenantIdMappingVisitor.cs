using Milvasoft.Core.EntityBases.MultiTenancy;
using System.Linq.Expressions;
using System.Reflection;

namespace Milvasoft.Core.Utils.ExpressionVisitors;

/// <summary>
/// Adds TenantId mapping for projections implementing IHasTenantId, including nested objects.
/// </summary>
public class TenantIdMappingVisitor(ParameterExpression rootParameter) : ExpressionVisitor
{
    private const string _tenantIdPropertyName = nameof(IHasTenantId.TenantId);
    private readonly ParameterExpression _rootParameter = rootParameter;

    /// <inheritdoc />
    protected override Expression VisitMemberInit(MemberInitExpression node)
    {
        if (!typeof(IHasTenantId).IsAssignableFrom(node.Type))
            return base.VisitMemberInit(node);

        var existingBindings = node.Bindings.OfType<MemberAssignment>().ToList();
        var tenantIdProperty = node.Type.GetProperty(_tenantIdPropertyName, BindingFlags.Public | BindingFlags.Instance);

        if (tenantIdProperty != null && !existingBindings.Exists(b => b.Member.Name == _tenantIdPropertyName))
        {
            var tenantIdExpression = Expression.Property(_rootParameter, _tenantIdPropertyName);
            var newAssignment = Expression.Bind(tenantIdProperty, tenantIdExpression);
            existingBindings.Add(newAssignment);
        }

        var newBindings = existingBindings.Select(binding =>
        {
            var expression = Visit(binding.Expression);

            if (typeof(IHasTenantId).IsAssignableFrom(binding.Expression.Type))
                expression = Visit(expression);

            return Expression.Bind(binding.Member, expression);
        }).ToList();

        var newExpression = Visit(node.NewExpression) as NewExpression;

        return Expression.MemberInit(newExpression, newBindings);
    }
}