using System;

namespace Milvasoft.SampleAPI.Entity
{

    /// <summary>
    /// Assignment for student.
    /// </summary>
    public class Assignment : EducationEntityBase
    {

        /// <summary>
        /// Tittle of assignment.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Description of assignment.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Remarks for student.
        /// </summary>
        public string RemarksToStudent { get; set; }

        /// <summary>
        /// Remarks for mentor.
        /// </summary>
        public string RemarksToMentor { get; set; }

        /// <summary>
        /// Difficulty level of the assignment.
        /// </summary>
        public int Level { get; set; }

        /// <summary>
        /// Rules of assignment.
        /// </summary>
        public string Rules { get; set; }

        /// <summary> 
        /// The maximum time that the assignment will be delivered.
        /// </summary>
        public int MaxDeliveryDay { get; set; }

        /// <summary>
        /// The profession Id of assignment.
        /// </summary>
        public Guid ProfessionId { get; set; }

        /// <summary>
        /// The profession of assignment.
        /// </summary>
        public virtual Profession Profession { get; set; }

    }
}
