﻿using Milvasoft.SampleAPI.Entity.Enum;
using System;

namespace Milvasoft.SampleAPI.Entity
{

    /// <summary>
    /// Student assginment relationship.
    /// </summary>
    public class StudentAssigment : EducationEntityBase
    {
        /// <summary>
        /// Student id.
        /// </summary>
        public Guid StudentId { get; set; }

        /// <summary>
        /// Student.
        /// </summary>
        public virtual Student Student { get; set; }

        /// <summary>
        /// Assignment id.
        /// </summary>
        public Guid AssigmentId { get; set; }

        /// <summary>
        /// Assignment.
        /// </summary>
        public virtual Assignment Assigment { get; set; }

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
        public bool IsApproved { get; set; }


    }
}
