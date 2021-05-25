using Milvasoft.Helpers.DataAccess.Concrete.Entity;
using Milvasoft.SampleAPI.DTOs.AssignmentDTOs;
using Milvasoft.SampleAPI.DTOs.StudentDTOs;
using Milvasoft.SampleAPI.Entity;
using Milvasoft.SampleAPI.Entity.Enum;
using Milvasoft.SampleAPI.Utils.Attributes.ValidationAttributes;
using Milvasoft.SampleAPI.Utils.Swagger;
using System;

namespace Milvasoft.SampleAPI.DTOs.StudentAssignmentDTOs
{
    /// <summary>
    /// Student assginment relationship.
    /// </summary>
    public class StudentAssignmentDTO : AuditableEntity<AppUser, Guid, Guid>
    {
        /// <summary>
        /// Student id.
        /// </summary>
        [OValidateId]
        public Guid StudentId { get; set; }

        /// <summary>
        /// Student.
        /// </summary>
        [SwaggerExclude]
        public virtual StudentDTO Student { get; set; }

        /// <summary>
        /// Assignment id.
        /// </summary>
        [OValidateId]
        public Guid AssigmentId { get; set; }

        /// <summary>
        /// Assignment.
        /// </summary>
        [SwaggerExclude]
        public virtual AssignmentDTO Assigment { get; set; }

        /// <summary>
        /// Education status.
        /// </summary>
        public EducationStatus Status { get; set; }

        /// <summary>
        /// The mentor's score on the student's assignment.
        /// </summary>
        public int MentorScore { get; set; }

        /// <summary>
        /// The mentor's description on the student's assignment.
        /// </summary>
        [OValidateString(2000)]
        public string MentorDescription { get; set; }

        /// <summary>
        /// File path of assignment.
        /// </summary>
        [OValidateString(2000)]
        public string AssigmentFilePath { get; set; }

        /// <summary>
        /// The date the student should submit the assignment.
        /// </summary>
        public DateTime ShouldDeliveryDate { get; set; }

        /// <summary>
        /// Assignment start date.
        /// </summary>
        public DateTime StartedDate { get; set; }

        /// <summary>
        /// Assignment finished date.
        /// </summary>
        public DateTime FinishedDate { get; set; }

        /// <summary>
        /// The additional time the student asks for the mentor.
        /// </summary>
        public int AdditionalTime { get; set; }

        /// <summary>
        /// An explanation of why the student is asking for additional time.
        /// </summary>
        public string AdditionalTimeDescription { get; set; }

        /// <summary>
        /// The status of the student's homework approved by the mentor.
        /// </summary>
        public bool IsActive { get; set; }
    }
}
