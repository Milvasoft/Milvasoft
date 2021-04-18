using Milvasoft.Helpers.Extensions;
using Milvasoft.SampleAPI.Entity;
using Milvasoft.SampleAPI.Spec.Abstract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Milvasoft.SampleAPI.Spec
{

    /// <summary>
    /// Filtering assignment object lists.
    /// </summary>
    public class AssignmentSpec : IBaseSpec<Assignment>
    {
        #region Fields
        private string _tittle;
        private string _rules;
        #endregion
        #region Props

        /// <summary>
        /// Tittle of assignment.
        /// </summary>
        public string Title { get => _tittle; set => _tittle = value?.ToUpper(); }

        /// <summary>
        /// Difficulty level of the assignment.
        /// </summary>
        public int? Level { get; set; }

        /// <summary>
        /// Rules of assignment.
        /// </summary>
        public string Rules { get => _tittle; set => _tittle = value?.ToUpper(); }

        /// <summary> 
        /// The maximum time that the assignment will be delivered.
        /// </summary>
        public int? MaxDeliveryDay { get; set; }

        /// <summary>
        /// The profession Id of assignment.
        /// </summary>
        public Guid? ProfessionId { get; set; }

        #endregion

        /// <summary>
        /// Filtering Assignment list by  with requested properties.
        /// </summary>
        /// <param name="entities"></param>
        /// <returns></returns>
        public List<Assignment> GetFilteredEntities(IEnumerable<Assignment> entities)
        {

            if (!Title.IsNullOrEmpty()) entities = entities.Where(m => m.Title.ToUpper().Contains(Title));
            if (!Rules.IsNullOrEmpty()) entities = entities.Where(m => m.Rules.ToUpper().Contains(Rules));

            if (Level.HasValue) entities.Where(m => m.Level == Level);
            if (MaxDeliveryDay.HasValue) entities.Where(m => m.MaxDeliveryDay == MaxDeliveryDay);

            if (ProfessionId.HasValue) entities = entities.Where(i => i.ProfessionId == ProfessionId);

            return entities.ToList();
        }

        /// <summary>
        /// Converts spesifications to expression.
        /// </summary>
        /// <returns></returns>
        public Expression<Func<Assignment, bool>> ToExpression()
        {

            Expression<Func<Assignment, bool>> mainPredicate = null;
            List<Expression<Func<Assignment, bool>>> predicates = new List<Expression<Func<Assignment, bool>>>();

            if (!string.IsNullOrEmpty(Title)) predicates.Add(c => c.Title == Title);
            if (!string.IsNullOrEmpty(Rules)) predicates.Add(c => c.Rules == Rules);


            if (Level.HasValue) predicates.Add(c => c.Level == Level);
            if (MaxDeliveryDay.HasValue) predicates.Add(c => c.MaxDeliveryDay == MaxDeliveryDay);
            if (ProfessionId.HasValue) predicates.Add(c => c.ProfessionId == ProfessionId);

            predicates?.ForEach(predicate => mainPredicate = mainPredicate.Append(predicate, ExpressionType.AndAlso));
            return mainPredicate;

        }
    }
}
