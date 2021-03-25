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
    /// Filtering profession object lists.
    /// </summary>
    public class MentorSpec : IBaseSpec<Mentor>
    {
        #region Props

        /// <summary>
        /// Mentor name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Mentor surname.
        /// </summary>
        public string Surname { get; set; }

        /// <summary>
        /// Professions of a mentor.
        /// </summary>
        public virtual IEnumerable<MentorProfession> Professions { get; set; }

        #endregion

        /// <summary>
        /// Filtering Assignment list by  with requested properties.
        /// </summary>
        /// <param name="entities"></param>
        /// <returns></returns>
        public List<Mentor> GetFilteredEntities(IEnumerable<Mentor> entities)
        {

            entities.ThrowIfListIsNullOrEmpty();

            return entities.ToList();
        }

        /// <summary>
        /// Converts spesifications to expression.
        /// </summary>
        /// <returns></returns>
        public Expression<Func<Mentor, bool>> ToExpression()
        {

            Expression<Func<Mentor, bool>> mainPredicate = null;
            List<Expression<Func<Mentor, bool>>> predicates = new List<Expression<Func<Mentor, bool>>>();

            if (!string.IsNullOrEmpty(Name)) predicates.Add(c => c.Name == Name);
            if (!string.IsNullOrEmpty(Surname)) predicates.Add(c => c.Surname == Surname);

            if (!Professions.IsNullOrEmpty()) predicates.Add(c => c.Professions == Professions);

            predicates?.ForEach(predicate => mainPredicate = mainPredicate.Append(predicate, ExpressionType.AndAlso));
            return mainPredicate;

        }
    }
}
