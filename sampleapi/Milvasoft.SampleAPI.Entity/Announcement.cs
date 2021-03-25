using System;

namespace Milvasoft.SampleAPI.Entity
{

    /// <summary>
    /// Announcements for all students.
    /// </summary>
    public class Announcement : EducationEntityBase
    {

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
        public bool IsFixed { get; set; }

        /// <summary>
        /// ID of the announcement mentor.
        /// </summary>
        public Guid MentorId { get; set; }

        /// <summary>
        /// Mentor of announcement.
        /// </summary>
        public virtual Mentor PublisherMentor { get; set; }

    }
}
