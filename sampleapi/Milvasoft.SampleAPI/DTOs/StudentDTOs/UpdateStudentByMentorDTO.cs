using Milvasoft.SampleAPI.Entity.Enum;
using Milvasoft.SampleAPI.Utils.Attributes.ValidationAttributes;
using System;

namespace Milvasoft.SampleAPI.DTOs.StudentDTOs
{
    /// <summary>
    /// DTO to be used in cases where the student needs to be updated by the mentor.
    /// </summary>
    public class UpdateStudentByMentorDTO
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
        /// The student's starting level of homework.
        /// </summary>
        [OValidateDecimal(20)]
        public int Level { get; set; }

        /// <summary>
        /// Student's university.
        /// </summary>
        [OValidateString(2, 200)]
        public string University { get; set; }

        /// <summary>
        /// Age of student.
        /// </summary>
        [OValidateDecimal(30)]
        public int Age { get; set; }

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
        /// Education status of student.
        /// </summary>
        public EducationStatus GraduationStatus { get; set; }

        /// <summary>
        /// Due date of current assignment.
        /// </summary>
        public DateTime CurrentAssigmentDeliveryDate { get; set; }

        /// <summary>
        /// Gradution score of student.
        /// </summary>
        [OValidateDecimal(100)]
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
    }
}
