using Milvasoft.Helpers;
using Milvasoft.Helpers.Exceptions;
using Milvasoft.Helpers.Extensions;
using Milvasoft.SampleAPI.AppStartup;
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
    /// Filtering announcement object lists.
    /// </summary>
    public class AnnouncementSpec : IBaseSpec<Announcement>
    {
        #region Props
        /// <summary>
        /// Tittle of announcement.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Description of announcement.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Is the announcement fixed?
        /// </summary>
        public bool? IsFixed { get; set; }

        /// <summary>
        /// ID of the announcement mentor.
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

            if (!string.IsNullOrEmpty(Title)) predicates.Add(a => a.Title== Title);
            if (!string.IsNullOrEmpty(Description)) predicates.Add(a => a.Description == Description);
            if (IsFixed.HasValue) predicates.Add(a => a.IsFixed == IsFixed);

            if (MentorId.HasValue) predicates.Add(a => a.MentorId == MentorId);

            predicates?.ForEach(predicate => mainPredicate = mainPredicate.Append(predicate, ExpressionType.AndAlso));
            return mainPredicate;
        }
    }
}
