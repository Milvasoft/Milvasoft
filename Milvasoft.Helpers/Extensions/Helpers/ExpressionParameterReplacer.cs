﻿using System.Collections.Generic;
using System.Linq.Expressions;

namespace Milvasoft.Helpers.Extensions.Helpers
{
    /// <summary>
    /// Expression parameter replacer for expression builder.
    /// </summary>
    public class ExpressionParameterReplacer : ExpressionVisitor
    {
        private IDictionary<ParameterExpression, ParameterExpression> ParameterReplacements { get; set; }

        /// <summary>
        /// Constructor of <see cref="ExpressionParameterReplacer"/>
        /// </summary>
        /// <param name="fromParameters"></param>
        /// <param name="toParameters"></param>
        public ExpressionParameterReplacer(IList<ParameterExpression> fromParameters, IList<ParameterExpression> toParameters)
        {
            ParameterReplacements = new Dictionary<ParameterExpression, ParameterExpression>();

            for (var i = 0; i != fromParameters.Count && i != toParameters.Count; i++)
            { ParameterReplacements.Add(fromParameters[i], toParameters[i]); }
        }

        /// <summary>
        /// Visits the <see cref="ParameterExpression"/>.
        /// </summary>
        /// <param name="node"> The expression to visit. </param>
        /// <returns> The modified expression, if it or any subexpression was modified; otherwise, returns the original expression.</returns>
        protected override Expression VisitParameter(ParameterExpression node)
        {
            if (ParameterReplacements.TryGetValue(node, out var replacement))
            { node = replacement; }

            return base.VisitParameter(node);
        }
    }
}
