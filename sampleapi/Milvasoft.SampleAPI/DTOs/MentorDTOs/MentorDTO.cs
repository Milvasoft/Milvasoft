using Milvasoft.Helpers.DataAccess.Concrete.Entity;
using Milvasoft.SampleAPI.DTOs.AnnouncementDTOs;
using Milvasoft.SampleAPI.DTOs.StudentDTOs;
using Milvasoft.SampleAPI.Entity;
using Milvasoft.SampleAPI.Utils.Attributes.ValidationAttributes;
using Milvasoft.SampleAPI.Utils.Swagger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Milvasoft.SampleAPI.DTOs.MentorDTOs
{

    /// <summary>
    /// Mentor DTO.
    /// </summary>
    public class MentorDTO : FullAuditableEntity<Guid>
    {
        /// <summary>
        /// Mentor name.
        /// </summary>
        [OValidateString(2,100)]
        public string Name { get; set; }

        /// <summary>
        /// Mentor surname.
        /// </summary>
        [OValidateString(2,100)]
        public string Surname { get; set; }

        /// <summary>
        /// CV path of mentor.
        /// </summary>
        [OValidateString(200)]
        public string CVFilePath { get; set; }

        /// <summary>
        /// AppUser ID of mentor.
        /// </summary>
        [OValidateId]
        public Guid AppUserId { get; set; }

        /// <summary>
        /// AppUser of mentor.
        /// </summary>
        [SwaggerExclude]
        public virtual AppUserDTO AppUser { get; set; }

        /// <summary>
        /// Announcements posted by the mentor.
        /// </summary>
        public virtual List<AnnouncementDTO> PublishedAnnouncements { get; set; }

        /// <summary>
        /// Professions of a mentor.
        /// </summary>
        public virtual List<MentorProfessionDTO> Professions { get; set; }

        /// <summary>
        /// Students of a mentor.
        /// </summary>
        public virtual List<StudentDTO> Students { get; set; }
    }
}
