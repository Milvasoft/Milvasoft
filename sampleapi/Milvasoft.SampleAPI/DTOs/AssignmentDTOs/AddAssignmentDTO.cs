﻿using Milvasoft.SampleAPI.Utils.Attributes.ValidationAttributes;
using System;

namespace Milvasoft.SampleAPI.DTOs.AssignmentDTOs
{
    /// <summary>
    /// AddAssignmentDTO for add operations.
    /// </summary>
    public class AddAssignmentDTO
    {
        /// <summary>
        /// Tittle of assignment.
        /// </summary>
        [OValidateString(10, 200)]
        public string Title { get; set; }

        /// <summary>
        /// Description of assignment.
        /// </summary>
        [OValidateString(2000)]
        public string Description { get; set; }

        /// <summary>
        /// Remarks for student.
        /// </summary>
        [OValidateString(2000)]
        public string RemarksToStudent { get; set; }

        /// <summary>
        /// Remarks for mentor.
        /// </summary>
        [OValidateString(2000)]
        public string RemarksToMentor { get; set; }

        /// <summary>
        /// Difficulty level of the assignment.
        /// </summary>
        [OValidateDecimal(20)]
        public int Level { get; set; }

        /// <summary>
        /// Rules of assignment.
        /// </summary>
        [OValidateString(2000)]
        public string Rules { get; set; }

        /// <summary> 
        /// The maximum time that the assignment will be delivered.
        /// </summary>
        [OValidateDecimal(20)]
        public int MaxDeliveryDay { get; set; }

        /// <summary>
        /// The profession Id of assignment.
        /// </summary>
        [OValidateId]
        public Guid ProfessionId { get; set; }
    }
}
