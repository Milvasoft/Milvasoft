using Milvasoft.SampleAPI.Utils.Attributes.ValidationAttributes;
using System;

namespace Milvasoft.SampleAPI.DTOs.AnnouncementDTOs
{
    /// <summary>
    /// UpdateAnnouncementDTO for update operations.
    /// </summary>
    public class UpdateAnnouncementDTO
    {
        /// <summary>
        /// Id of updated announcement.
        /// </summary>
        public Guid Id { get; set; }

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
    }
}
