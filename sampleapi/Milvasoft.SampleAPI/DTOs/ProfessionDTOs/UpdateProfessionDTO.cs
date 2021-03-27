using Milvasoft.SampleAPI.Entity;
using Milvasoft.SampleAPI.Utils.Attributes.ValidationAttributes;

namespace Milvasoft.SampleAPI.DTOs.ProfessionDTOs
{
    /// <summary>
    /// AddProfessionDTO for add operations.
    /// </summary>
    public class UpdateProfessionDTO : EducationEntityBase
    {
        /// <summary>
        /// Name of profession.
        /// </summary>
        [OValidateString(2, 200)]
        public string Name { get; set; }
    }
}
