using Milvasoft.SampleAPI.DTOs.ProfessionDTOs;
using Milvasoft.SampleAPI.Entity;
using Milvasoft.SampleAPI.Utils.Attributes.ValidationAttributes;
using Milvasoft.SampleAPI.Utils.Swagger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Milvasoft.SampleAPI.DTOs.AssignmentDTOs
{

    /// <summary>
    /// Assignment for student.
    /// </summary>
    public class AssignmentDTO : EducationEntityBase
    {

        /// <summary>
        /// Tittle of assignment.
        /// </summary>
        [OValidateString(200)]
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
        public int Level { get; set; }

        /// <summary>
        /// Rules of assignment.
        /// </summary>
        [OValidateString(2000)]
        public string Rules { get; set; }

        /// <summary> 
        /// The maximum time that the assignment will be delivered.
        /// </summary>
        public int MaxDeliveryDay { get; set; }

        /// <summary>
        /// The profession Id of assignment.
        /// </summary>
        [OValidateId]
        public Guid ProfessionId { get; set; }

        /// <summary>
        /// The profession of assignment.
        /// </summary>
        [SwaggerExclude]
        public virtual ProfessionDTO Profession { get; set; }
    }
}
