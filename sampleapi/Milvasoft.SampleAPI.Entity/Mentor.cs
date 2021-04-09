using Milvasoft.Helpers.DataAccess.Concrete.Entity;
using System;
using System.Collections.Generic;

namespace Milvasoft.SampleAPI.Entity
{

    /// <summary>
    /// Mentor entity.
    /// </summary>
    public class Mentor : FullAuditableEntity<Guid>
    {
        /// <summary>
        /// Mentor name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Mentor surname.
        /// </summary>
        public string Surname { get; set; }

        /// <summary>
        /// CV path of mentor.
        /// </summary>
        public string CVFilePath { get; set; }

        /// <summary>
        /// AppUser ID of mentor.
        /// </summary>
        public Guid AppUserId { get; set; }

        /// <summary>
        /// AppUser of mentor.
        /// </summary>
        public virtual AppUser AppUser { get; set; }

        /// <summary>
        /// Announcements posted by the mentor.
        /// </summary>
        public virtual IEnumerable<Announcement> PublishedAnnouncements { get; set; }

        /// <summary>
        /// Professions of a mentor.
        /// </summary>
        public virtual IEnumerable<MentorProfession> Professions { get; set; }

        /// <summary>
        /// Students of a mentor.
        /// </summary>
        public virtual IEnumerable<Student> Students { get; set; }
    }
}