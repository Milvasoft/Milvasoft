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
    /// Filtering question object lists.
    /// </summary>
    public class QuestionSpec : IBaseSpec<Question>
    {
        private string _tittle;

        #region Props
        /// <summary>
        /// Tittle of question.
        /// </summary>
        public string Title { get => _tittle; set => _tittle = value?.ToUpper(); }

        /// <summary>
        /// Is the question useful?
        /// </summary>
        public bool? IsUseful { get; set; }

        /// <summary>
        /// Will the question be shown as a useful question?
        /// </summary>
        public bool? WillShown { get; set; }

        /// <summary>
        /// Profession ıd of the question.
        /// </summary>
        public Guid? ProfessionId { get; set; }


        /// <summary>
        /// Id of the student who asked the question.
        /// </summary>
        public Guid? StudentId { get; set; }

        /// <summary>
        /// Id of the mentor answering the question.
        /// </summary>
        public Guid? MentorId { get; set; }

        #endregion

        /// <summary>
        /// Filtering question list by  with requested properties.
        /// </summary>
        /// <param name="entities"></param>
        /// <returns></returns>
        public List<Question> GetFilteredEntities(IEnumerable<Question> entities)
        {
            if (!Title.IsNullOrEmpty()) entities = entities.Where(m => m.Title.ToUpper().Contains(Title));

            if (MentorId.HasValue) entities = entities.Where(i => i.MentorId == MentorId);
            if (ProfessionId.HasValue) entities = entities.Where(i => i.ProfessionId == ProfessionId);
            if (StudentId.HasValue) entities = entities.Where(i => i.StudentId == StudentId);

            if (WillShown.HasValue) entities = entities.Where(i => i.WillShown == WillShown);
            if (IsUseful.HasValue) entities = entities.Where(i => i.IsUseful == IsUseful);

            return entities.ToList();
        }

        /// <summary>
        /// Converts spesifications to expression.
        /// </summary>
        /// <returns></returns>
        public Expression<Func<Question, bool>> ToExpression()
        {
            Expression<Func<Question, bool>> mainPredicate = null;
            List<Expression<Func<Question, bool>>> predicates = new List<Expression<Func<Question, bool>>>();

            if (!string.IsNullOrEmpty(Title)) predicates.Add(c => c.Title == Title);

            if (IsUseful.HasValue) predicates.Add(c => c.IsUseful == IsUseful);
            if (WillShown.HasValue) predicates.Add(c => c.WillShown == WillShown);

            if (ProfessionId.HasValue) predicates.Add(c => c.ProfessionId == ProfessionId);
            if (StudentId.HasValue) predicates.Add(c => c.StudentId == StudentId);
            if (MentorId.HasValue) predicates.Add(c => c.MentorId == MentorId);

            predicates?.ForEach(predicate => mainPredicate = mainPredicate.Append(predicate, ExpressionType.AndAlso));
            return mainPredicate;
        }
    }
}
