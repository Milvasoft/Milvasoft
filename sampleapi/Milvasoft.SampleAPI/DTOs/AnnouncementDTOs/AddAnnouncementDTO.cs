using Milvasoft.SampleAPI.Utils.Attributes.ValidationAttributes;
using System;

namespace Milvasoft.SampleAPI.DTOs.AnnouncementDTOs
{
    /// <summary>
    /// AddAnnouncementDTO for add operations.
    /// </summary>
    public class AddAnnouncementDTO
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
    }
}
