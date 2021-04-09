using Milvasoft.Helpers.DataAccess.Concrete.Entity;
using Milvasoft.SampleAPI.DTOs.MentorDTOs;
using Milvasoft.SampleAPI.Entity;
using Milvasoft.SampleAPI.Utils.Attributes.ValidationAttributes;
using Milvasoft.SampleAPI.Utils.Swagger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Milvasoft.SampleAPI.DTOs.AnnouncementDTOs
{
    /// <summary>
    /// Announcements for all students.
    /// </summary>
    public class AnnouncementForStudentDTO : AuditableEntity<AppUser, Guid, Guid>
    {
        /// <summary>
        /// Tittle of announcement.
        /// </summary>
        [OValidateString(200)]
        public string Title { get; set; }

        /// <summary>
        /// Description of announcement.
        /// </summary>
        [OValidateString(2000)]
        public string Description { get; set; }

        /// <summary>
        /// Mentor of announcement.
        /// </summary>
        [SwaggerExclude]
        public virtual MentorDTO PublisherMentor { get; set; }
    }
}
