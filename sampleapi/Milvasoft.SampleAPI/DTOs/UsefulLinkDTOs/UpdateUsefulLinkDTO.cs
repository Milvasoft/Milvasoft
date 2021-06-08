using Milvasoft.SampleAPI.Utils.Attributes.ValidationAttributes;
using System;

namespace Milvasoft.SampleAPI.DTOs.UsefulLinkDTOs
{
    /// <summary>
    /// UpdateUsefulLinkDTO for update useful link operations.
    /// </summary>
    public class UpdateUsefulLinkDTO
    {
        /// <summary>
        /// Id of to be updated useful links.
        /// </summary>
        [OValidateId]
        public Guid Id { get; set; }

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
        [OValidateId]
        public Guid ProfessionId { get; set; }
    }
}
