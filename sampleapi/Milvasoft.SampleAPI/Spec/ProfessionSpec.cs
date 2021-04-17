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
    /// Filtering profession object lists.
    /// </summary>
    public class ProfessionSpec : IBaseSpec<Profession>
    {
        #region Fields
        private string _name;
        #endregion
        #region Props

        /// <summary>
        /// Name of profession.
        /// </summary>
        public string Name { get => _name; set => _name = value?.ToUpper(); }
        #endregion

        /// <summary>
        /// Filtering profession list by  with requested properties.
        /// </summary>
        /// <param name="entities"></param>
        /// <returns></returns>
        public List<Profession> GetFilteredEntities(IEnumerable<Profession> entities)
        {

            if (!Name.IsNullOrEmpty()) entities = entities.Where(m => m.Name.ToUpper().Contains(Name));

            return entities.ToList();

        }

        /// <summary>
        /// Converts spesifications to expression.
        /// </summary>
        /// <returns></returns>
        public Expression<Func<Profession, bool>> ToExpression()
        {

            Expression<Func<Profession, bool>> mainPredicate = null;
            List<Expression<Func<Profession, bool>>> predicates = new List<Expression<Func<Profession, bool>>>();

            if (!string.IsNullOrEmpty(Name)) predicates.Add(c => c.Name == Name);

            predicates?.ForEach(predicate => mainPredicate = mainPredicate.Append(predicate, ExpressionType.AndAlso));
            return mainPredicate;

        }
    }
}
