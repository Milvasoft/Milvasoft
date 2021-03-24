using Milvasoft.Helpers.DataAccess.Concrete.Entity;
using Milvasoft.SampleAPI.DTOs.MentorDTOs;
using Milvasoft.SampleAPI.DTOs.ProfessionDTOs;
using Milvasoft.SampleAPI.Entity;
using Milvasoft.SampleAPI.Entity.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
        public string Name { get; set; }

        /// <summary>
        /// Student's surname.
        /// </summary>
        public string Surname { get; set; }

        /// <summary>
        /// Student's university.
        /// </summary>
        public string University { get; set; }

        /// <summary>
        /// Age of student.
        /// </summary>
        public int Age { get; set; }

        /// <summary>
        /// Dream of student.
        /// </summary>
        public string Dream { get; set; }

        /// <summary>
        /// Home adress of student.
        /// </summary>
        public string HomeAddress { get; set; }

        /// <summary>
        /// The mentor's thoughts about the student.
        /// </summary>
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
        public string MentorGraduationThoughts { get; set; }

        /// <summary>
        /// Due date of current assignment.
        /// </summary>
        public DateTime CurrentAssigmentDeliveryDate { get; set; }

        /// <summary>
        /// AppUser id.
        /// </summary>
        public Guid AppUserId { get; set; }

        /// <summary>
        /// AppUser of student.
        /// </summary>
        public virtual AppUserDTO AppUser { get; set; }

        /// <summary>
        /// Profession id of student.
        /// </summary>
        public Guid ProfessionId { get; set; }

        /// <summary>
        /// Profesion of student.
        /// </summary>
        public virtual ProfessionDTO Profession { get; set; }

        /// <summary>
        /// Mentor ıd of student.
        /// </summary>
        public Guid MentorId { get; set; }

        /// <summary>
        /// Mentor of student.
        /// </summary>
        public virtual MentorDTO Mentor { get; set; }

        /// <summary>
        /// Old assignments of student.
        /// </summary>
        public virtual List<StudentAssigmentDTO> OldAssignments { get; set; }



        /// <summary>
        /// Number of previous successful assignments.
        /// </summary>
        public int SuccesAssignmentCount
        {
            get
            {
                return OldAssignments?.Count(i => i.Status == EducationStatus.Success) ?? 0;
            }
        }

        /// <summary>
        /// Number of previous failed assignments.
        /// </summary>
        public int FailedAssignmentCount 
        {
            get
            {
                 return OldAssignments?.Count(i => i.Status == EducationStatus.Fail) ?? 0;
            }
        }
    }
}
