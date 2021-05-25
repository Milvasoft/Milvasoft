using Milvasoft.SampleAPI.Utils.Attributes.ValidationAttributes;
using System;

namespace Milvasoft.SampleAPI.DTOs.StudentDTOs
{
    /// <summary>
    /// DTO to be used when the student needs to be updated by the admin.
    /// </summary>
    public class UpdateStudentByAdminDTO
    {
        /// <summary>
        /// Student's name.
        /// </summary>
        [OValidateString(2, 200)]
        public string Name { get; set; }

        /// <summary>
        /// Student's surname.
        /// </summary>
        [OValidateString(2, 200)]
        public string Surname { get; set; }

        /// <summary>
        /// Student's university.
        /// </summary>
        [OValidateString(2, 200)]
        public string University { get; set; }

        /// <summary>
        /// Age of student.
        /// </summary>
        public int Age { get; set; }

        /// <summary>
        /// Did the student sign the contract?
        /// </summary>
        public bool IsConfidentialityAgreementSigned { get; set; }

        /// <summary>
        /// Mentor ıd of student.
        /// </summary>
        [OValidateId]
        public Guid MentorId { get; set; }
    }
}
