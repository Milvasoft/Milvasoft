using Microsoft.AspNetCore.Http;
using Milvasoft.SampleAPI.Utils.Attributes.ValidationAttributes;

namespace Milvasoft.SampleAPI.DTOs.MentorDTOs
{
    /// <summary>
    /// Update mentor DTO for update operations.
    /// </summary>
    public class UpdateMentorDTO
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
        /// CV path of mentor.
        /// </summary>
        public IFormFile CVFilePath { get; set; }

        /// <summary>
        /// User profile pictures.
        /// </summary>
        public IFormFile Photo { get; set; }

    }
}
