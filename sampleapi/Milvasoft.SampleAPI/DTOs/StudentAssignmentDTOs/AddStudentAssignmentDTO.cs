using Milvasoft.SampleAPI.Utils.Attributes.ValidationAttributes;

namespace Milvasoft.SampleAPI.DTOs.StudentAssignmentDTOs
{
    /// <summary>
    /// Dto to use when adding a student assignment.
    /// </summary>
    public class AddStudentAssignmentDTO
    {
        /// <summary>
        /// The additional time the student asks for the mentor.
        /// </summary>
        [OValidateDecimal(10)]
        public int AdditionalTime { get; set; }

        /// <summary>
        /// An explanation of why the student is asking for additional time.
        /// </summary>
        [OValidateString(2000)]
        public string AdditionalTimeDescription { get; set; }
    }
}
