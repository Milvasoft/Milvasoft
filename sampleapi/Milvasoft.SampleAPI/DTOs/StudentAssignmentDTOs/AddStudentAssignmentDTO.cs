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
        public int AdditionalTime { get; set; }

        /// <summary>
        /// An explanation of why the student is asking for additional time.
        /// </summary>
        public string AdditionalTimeDescription { get; set; }
    }
}
