using Milvasoft.Helpers.Extensions;
using Milvasoft.SampleAPI.Entity;
using Milvasoft.SampleAPI.Spec.Abstract;
using Milvasoft.SampleAPI.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Milvasoft.SampleAPI.Spec
{

    /// <summary>
    /// Filtering Useful Link object list.
    /// </summary>
    public class UsefulLinkSpec : IBaseSpec<UsefulLink>
    {
        #region Props

        /// <summary>
        /// Tittle of link.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Profession id of link.
        /// </summary>
        public Guid? ProfessionId { get; set; }

        #endregion

        /// <summary>
        /// Filtering question list by  with requested properties.
        /// </summary>
        /// <param name="entities"></param>
        /// <returns></returns>
        public List<UsefulLink> GetFilteredEntities(IEnumerable<UsefulLink> entities)
        {

            entities.ThrowIfListIsNullOrEmpty();

            if (!string.IsNullOrEmpty(Title)) entities = entities.Where(c => c.Title == Title);

            if (ProfessionId.HasValue) entities = entities.Where(c => c.ProfessionId == ProfessionId);

            return entities.ToList();

        }

        /// <summary>
        /// Converts spesifications to expression.
        /// </summary>
        /// <returns></returns>
        public Expression<Func<UsefulLink, bool>> ToExpression()
        {

            Expression<Func<UsefulLink, bool>> mainPredicate = null;
            List<Expression<Func<UsefulLink, bool>>> predicates = new List<Expression<Func<UsefulLink, bool>>>();

            if (!string.IsNullOrEmpty(Title)) predicates.Add(c => c.Title == Title);

            if (ProfessionId.HasValue) predicates.Add(c => c.ProfessionId == ProfessionId);

            predicates?.ForEach(predicate => mainPredicate = mainPredicate.Append(predicate, ExpressionType.AndAlso));
            return mainPredicate;


        }
    }
}
