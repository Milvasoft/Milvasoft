using Milvasoft.SampleAPI.DTOs.ProfessionDTOs;
using Milvasoft.SampleAPI.Utils.Attributes.ValidationAttributes;
using System;
using System.Collections.Generic;

namespace Milvasoft.SampleAPI.DTOs.MentorDTOs
{
    /// <summary>
    /// Add mentor DTO for add operations.
    /// </summary>
    public class AddMentorDTO
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
        public string CVFilePath { get; set; }

        /// <summary>
        /// Profession ıd of mentor.
        /// </summary>
        public List<AddProfessionDTO> Professions{ get; set; }
    }
}
