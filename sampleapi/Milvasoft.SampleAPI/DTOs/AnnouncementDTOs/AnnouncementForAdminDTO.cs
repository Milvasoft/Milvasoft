using Milvasoft.Helpers.DataAccess.Concrete.Entity;
using Milvasoft.SampleAPI.DTOs.MentorDTOs;
using Milvasoft.SampleAPI.Entity;
using Milvasoft.SampleAPI.Utils.Attributes.ValidationAttributes;
using Milvasoft.SampleAPI.Utils.Swagger;
using System;

namespace Milvasoft.SampleAPI.DTOs.AnnouncementDTOs
{
    /// <summary>
    /// Announcements for admin.
    /// </summary>
    public class AnnouncementForAdminDTO : AuditableEntity<AppUser, Guid, Guid>
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
        /// Is the announcement fixed?
        /// </summary>
        public bool IsFixed { get; set; }

        /// <summary>
        /// ID of the announcement mentor.
        /// </summary>
        [OValidateId]
        public Guid MentorId { get; set; }

        /// <summary>
        /// Mentor of announcement.
        /// </summary>
        [SwaggerExclude]
        public virtual MentorDTO PublisherMentor { get; set; }
    }
}
