using Milvasoft.Helpers.DataAccess.Concrete.Entity;
using Milvasoft.SampleAPI.DTOs.MentorDTOs;
using Milvasoft.SampleAPI.DTOs.ProfessionDTOs;
using Milvasoft.SampleAPI.Entity;
using Milvasoft.SampleAPI.Entity.Enum;
using Milvasoft.SampleAPI.Utils.Attributes.ValidationAttributes;
using Milvasoft.SampleAPI.Utils.Swagger;
using System;
using System.Collections.Generic;

namespace Milvasoft.SampleAPI.DTOs.StudentDTOs
{

    /// <summary>
    /// Student DTO.
    /// </summary>
    public class StudentDTO : FullAuditableEntity<Guid>
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
        /// Due date of current assignment.
        /// </summary>
        public DateTime CurrentAssigmentDeliveryDate { get; set; }

        /// <summary>
        /// AppUser id.
        /// </summary>
        [OValidateId]
        public Guid AppUserId { get; set; }

        /// <summary>
        /// AppUser of student.
        /// </summary>
        [SwaggerExclude]
        public virtual AppUserDTO AppUser { get; set; }

        /// <summary>
        /// Profession id of student.
        /// </summary>
        [OValidateId]
        public Guid ProfessionId { get; set; }

        /// <summary>
        /// Profesion of student.
        /// </summary>
        [SwaggerExclude]
        public virtual ProfessionDTO Profession { get; set; }

        /// <summary>
        /// Mentor ıd of student.
        /// </summary>
        [OValidateId]
        public Guid MentorId { get; set; }

        /// <summary>
        /// Mentor of student.
        /// </summary>
        [SwaggerExclude]
        public virtual MentorDTO Mentor { get; set; }

        /// <summary>
        /// Old assignments of student.
        /// </summary>
        public virtual List<StudentAssigmentDTO> OldAssignments { get; set; }
    }
}
