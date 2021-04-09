using Milvasoft.Helpers.DataAccess.Concrete.Entity;
using Milvasoft.SampleAPI.DTOs.AnnouncementDTOs;
using Milvasoft.SampleAPI.Entity;
using Milvasoft.SampleAPI.Utils.Attributes.ValidationAttributes;
using System;
using System.Collections.Generic;

namespace Milvasoft.SampleAPI.DTOs.MentorDTOs
{
    /// <summary>
    /// Mentor entities for student.
    /// </summary>
    public class MentorForStudentDTO : AuditableEntity<AppUser, Guid, Guid>
    {
        /// <summary>
        /// Mentor name.
        /// </summary>
        [OValidateString(2, 100)]
        public string Name { get; set; }

        /// <summary>
        /// Mentor surname.
        /// </summary>
        [OValidateString(2, 100)]
        public string Surname { get; set; }

        /// <summary>
        /// Announcements posted by the mentor.
        /// </summary>
        public virtual List<AnnouncementDTO> PublishedAnnouncements { get; set; }

        /// <summary>
        /// Professions of a mentor.
        /// </summary>
        public virtual List<MentorProfessionDTO> Professions { get; set; }
    }
}
