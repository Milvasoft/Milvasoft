using Milvasoft.SampleAPI.Utils.Attributes.ValidationAttributes;
using System;

namespace Milvasoft.SampleAPI.DTOs.MentorDTOs
{
    /// <summary>
    /// Update mentor DTO for update operations.
    /// </summary>
    public class UpdateMentorDTO
    {
        /// <summary>
        /// Id of updated mentor.
        /// </summary>
        public Guid Id { get; set; }

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
        /// CV path of mentor.
        /// </summary>
        public string CVFilePath { get; set; }

        /// <summary>
        /// AppUser ID of mentor.
        /// </summary>
    }
}
