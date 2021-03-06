﻿using Milvasoft.Helpers;
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
    public class MentorSpec : IBaseSpec<Mentor>
    {
        private string _name;
        private string _surname;

        #region Props

        /// <summary>
        /// Mentor name.
        /// </summary>
        public string Name { get => _name; set => _name = value?.ToUpper(); }

        /// <summary>
        /// Mentor surname.
        /// </summary>
        public string Surname { get => _surname; set => _surname = value?.ToUpper(); }

        /// <summary> 
        /// Low date of mentor.
        /// </summary>
        public DateTime? MentorLowDate { get; set; }

        /// <summary> 
        /// Top date of mentor.
        /// </summary>
        public DateTime? MentorTopDate { get; set; }

        #endregion

        /// <summary>
        /// Filtering Assignment list by  with requested properties.
        /// </summary>
        /// <param name="entities"></param>
        /// <returns></returns>
        public List<Mentor> GetFilteredEntities(IEnumerable<Mentor> entities)
        {
            if (!Name.IsNullOrEmpty()) entities = entities.Where(m => m.Name.ToUpper().Contains(Name));
            if (!Surname.IsNullOrEmpty()) entities = entities.Where(m => m.Surname.ToUpper().Contains(Surname));

            return entities.ToList();
        }

        /// <summary>
        /// Converts spesifications to expression.
        /// </summary>
        /// <returns></returns>
        public Expression<Func<Mentor, bool>> ToExpression()
        {
            Expression<Func<Mentor, bool>> mainPredicate = null;
            List<Expression<Func<Mentor, bool>>> predicates = new();

            if (!string.IsNullOrEmpty(Name)) predicates.Add(c => c.Name == Name);
            if (!string.IsNullOrEmpty(Surname)) predicates.Add(c => c.Surname == Surname);

            var dateExpression = Filter.CreateDateFilterExpression<Mentor>(MentorTopDate, MentorLowDate, i => i.CreationDate);
            if (dateExpression != null) predicates.Add(dateExpression);

            predicates?.ForEach(predicate => mainPredicate = mainPredicate.Append(predicate, ExpressionType.AndAlso));
            return mainPredicate;
        }
    }
}
