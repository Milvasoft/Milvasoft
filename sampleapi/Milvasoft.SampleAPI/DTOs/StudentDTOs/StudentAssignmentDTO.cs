using Milvasoft.SampleAPI.DTOs.AssignmentDTOs;
using Milvasoft.SampleAPI.Entity;
using Milvasoft.SampleAPI.Entity.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Milvasoft.SampleAPI.DTOs.StudentDTOs
{
    /// <summary>
    /// Student assginment relationship.
    /// </summary>
    public class StudentAssignmentDTO : EducationEntityBase
    {

        /// <summary>
        /// Student id.
        /// </summary>
        public Guid StudentId { get; set; }

        /// <summary>
        /// Student.
        /// </summary>
        public virtual StudentDTO Student { get; set; }



        /// <summary>
        /// Assignment id.
        /// </summary>
        public Guid AssigmentId { get; set; }

        /// <summary>
        /// Assignment.
        /// </summary>
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
        public string MentorDescription { get; set; }

        /// <summary>
        /// File path of assignment.
        /// </summary>
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
    }
}
