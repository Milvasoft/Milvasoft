using Milvasoft.Helpers.Extensions;
using Milvasoft.SampleAPI.Entity;
using Milvasoft.SampleAPI.Spec.Abstract;
using Milvasoft.SampleAPI.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Milvasoft.SampleAPI.Spec
{
    /// <summary>
    /// Filtering question object lists.
    /// </summary>
    public class QuestionSpec : IBaseSpec<Question>
    {
        #region Props
        /// <summary>
        /// Tittle of question.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Content of question.
        /// </summary>
        public string QuestionContent { get; set; }

        /// <summary>
        /// The mentor's answer to the question.
        /// </summary>
        public string MentorReply { get; set; }

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

            entities.ThrowIfListIsNullOrEmpty();

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
            if (!string.IsNullOrEmpty(QuestionContent)) predicates.Add(c => c.QuestionContent == QuestionContent);
            if (!string.IsNullOrEmpty(MentorReply)) predicates.Add(c => c.MentorReply== MentorReply);

            if (IsUseful.HasValue) predicates.Add(c => c.IsUseful== IsUseful);
            if (WillShown.HasValue) predicates.Add(c => c.WillShown == WillShown);
            if (ProfessionId.HasValue) predicates.Add(c => c.ProfessionId == ProfessionId);
            if (StudentId.HasValue) predicates.Add(c => c.StudentId== StudentId);
            if (MentorId.HasValue) predicates.Add(c => c.MentorId== MentorId);

            predicates?.ForEach(predicate => mainPredicate = mainPredicate.Append(predicate, ExpressionType.AndAlso));
            return mainPredicate;


        }
    }
}
