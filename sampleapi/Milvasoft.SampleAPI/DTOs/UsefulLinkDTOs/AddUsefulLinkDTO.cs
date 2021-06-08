using Milvasoft.SampleAPI.Utils.Attributes.ValidationAttributes;
using System;

namespace Milvasoft.SampleAPI.DTOs.UsefulLinkDTOs
{
    /// <summary>
    /// UpdateUsefulLinkDTO for add useful link operations.
    /// </summary>
    public class AddUsefulLinkDTO
    {
        /// <summary>
        /// Tittle of link.
        /// </summary>
        [OValidateString(100)]
        public string Title { get; set; }

        /// <summary>
        /// Description of link.
        /// </summary>
        [OValidateString(2000)]
        public string Description { get; set; }

        /// <summary>
        /// URL.
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// Profession id of link.
        /// </summary>
        public Guid ProfessionId { get; set; }
    }
}
