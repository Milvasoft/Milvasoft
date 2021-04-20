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
    /// Filtering announcement object lists.
    /// </summary>
    public class AnnouncementSpec : IBaseSpec<Announcement>
    {
        #region Fields
        private string _tittle;
        private string _description;
        #endregion
        #region Props
        /// <summary>
        /// Filtering by Tittle of Announcement.
        /// </summary>
        public string Title { get => _tittle; set => _tittle = value?.ToUpper(); }

        /// <summary>
        /// Filtering by Description of Announcement.
        /// </summary>
        public string Description { get => _description; set => _description = value?.ToUpper(); }

        /// <summary>
        /// Filtering by IsFixed of Announcement.
        /// </summary>
        public bool? IsFixed { get; set; }

        /// <summary>
        /// Filtering by MentorId of Announcement.
        /// </summary>
        public Guid? MentorId { get; set; }

        #endregion

        /// <summary>
        /// Filtering Announcement list by  with requested properties.
        /// </summary>
        /// <param name="entities"></param>
        /// <returns></returns>
        public List<Announcement> GetFilteredEntities(IEnumerable<Announcement> entities)
        {
            entities.ThrowIfListIsNullOrEmpty();
            return entities.ToList();
        }

        /// <summary>
        /// Converts spesifications to expression.
        /// </summary>
        /// <returns></returns>
        public Expression<Func<Announcement, bool>> ToExpression()
        {
            Expression<Func<Announcement, bool>> mainPredicate = null;
            List<Expression<Func<Announcement, bool>>> predicates = new List<Expression<Func<Announcement, bool>>>();

            if (!string.IsNullOrEmpty(Title)) predicates.Add(a => a.Title == Title);
            if (!string.IsNullOrEmpty(Description)) predicates.Add(a => a.Description == Description);
            if (IsFixed.HasValue) predicates.Add(a => a.IsFixed == IsFixed);

            if (MentorId.HasValue) predicates.Add(a => a.MentorId == MentorId);

            predicates?.ForEach(predicate => mainPredicate = mainPredicate.Append(predicate, ExpressionType.AndAlso));
            return mainPredicate;
        }
    }
}
