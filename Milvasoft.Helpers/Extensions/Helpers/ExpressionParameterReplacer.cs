using System.Collections.Generic;
using System.Linq.Expressions;

namespace Milvasoft.Helpers.Extensions.Helpers
{
    public class ExpressionParameterReplacer : ExpressionVisitor
    {
        private IDictionary<ParameterExpression, ParameterExpression> ParameterReplacements { get; set; }

        public ExpressionParameterReplacer(IList<ParameterExpression> fromParameters, IList<ParameterExpression> toParameters)
        {
            ParameterReplacements = new Dictionary<ParameterExpression, ParameterExpression>();

            for (var i = 0; i != fromParameters.Count && i != toParameters.Count; i++)
            { ParameterReplacements.Add(fromParameters[i], toParameters[i]); }
        }

        protected override Expression VisitParameter(ParameterExpression node)
        {
            if (ParameterReplacements.TryGetValue(node, out var replacement))
            { node = replacement; }

            return base.VisitParameter(node);
        }
    }
}
