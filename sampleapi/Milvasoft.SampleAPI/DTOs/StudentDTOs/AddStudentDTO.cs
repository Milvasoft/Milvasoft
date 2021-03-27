using Milvasoft.SampleAPI.Entity.Enum;
using Milvasoft.SampleAPI.Utils.Attributes.ValidationAttributes;
using System;

namespace Milvasoft.SampleAPI.DTOs.StudentDTOs
{
    /// <summary>
    /// AddStudentDTO for add student operations.
    /// </summary>
    public class AddStudentDTO
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
        /// Dream of student.
        /// </summary>
        [OValidateString(2000)]
        public string Dream { get; set; }

        /// <summary>
        /// Home adress of student.
        /// </summary>
        [OValidateString(2000)]
        public string HomeAddress { get; set; }

        /// <summary>
        /// The mentor's thoughts about the student.
        /// </summary>
        [OValidateString(2000)]
        public string MentorThoughts { get; set; }

        /// <summary>
        /// Did the student sign the contract?
        /// </summary>
        public bool IsConfidentialityAgreementSigned { get; set; }

        /// <summary>
        /// Education status of student.
        /// </summary>
        public EducationStatus GraduationStatus { get; set; }

        /// <summary>
        /// Gradution score of student.
        /// </summary>
        public int GraduationScore { get; set; }

        /// <summary>
        /// The mentor's graduation thoughts of student.
        /// </summary>
        [OValidateString(2000)]
        public string MentorGraduationThoughts { get; set; }

        /// <summary>
        /// Profession id of student.
        /// </summary>
        [OValidateId]
        public Guid ProfessionId { get; set; }

        /// <summary>
        /// Mentor ıd of student.
        /// </summary>
        [OValidateId]
        public Guid MentorId { get; set; }

    }
}
