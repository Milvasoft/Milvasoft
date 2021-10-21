using Milvasoft.Helpers;
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
        private string _name;

        /// <summary>
        /// Name of profession.
        /// </summary>
        public string Name { get => _name; set => _name = value?.ToUpper(); }

        /// <summary> 
        /// Low date of profession.
        /// </summary>
        public DateTime? ProfessionLowDate { get; set; }

        /// <summary> 
        /// Top date of profession.
        /// </summary>
        public DateTime? ProfessionTopDate { get; set; }

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

            if (!string.IsNullOrWhiteSpace(Name)) predicates.Add(c => c.Name.ToUpper() == Name.ToUpper());

            var dateExpression = Filter.CreateDateFilterExpression<Profession>(ProfessionTopDate, ProfessionLowDate, i => i.CreationDate);
            if (dateExpression != null) predicates.Add(dateExpression);

            predicates?.ForEach(predicate => mainPredicate = mainPredicate.Append(predicate, ExpressionType.AndAlso));
            return mainPredicate;
        }
    }
}
