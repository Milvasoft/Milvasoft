using Milvasoft.SampleAPI.DTOs.MentorDTOs;
using Milvasoft.SampleAPI.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Milvasoft.SampleAPI.DTOs.AnnouncementDTOs
{
    /// <summary>
    /// Announcements for all students.
    /// </summary>
    public class AnnouncementDTO : EducationEntityBase
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
        public virtual MentorDTO PublisherMentor { get; set; }
    }
}
