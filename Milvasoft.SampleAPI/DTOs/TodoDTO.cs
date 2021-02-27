using Milvasoft.SampleAPI.Entity;
using System;

namespace Milvasoft.SampleAPI.DTOs
{
    public class TodoDTO : BaseEntity
    {
        /// <summary>
        /// User defined task content.
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        /// Remind date of task.
        /// </summary>
        public DateTime? ReminMeDate { get; set; }

        /// <summary>
        /// Due date of task.
        /// </summary>
        public DateTime? DueDate { get; set; }

        public bool IsFavorite { get; set; }

        /// <summary>
        /// Foreing key of category.
        /// </summary>
        public Guid CategoryId { get; set; }

        public string CategoryName { get; set; }

        /// <summary>
        /// Navigation property of category.
        /// </summary>
        public virtual CategoryDTO Category { get; set; }
    }
}
