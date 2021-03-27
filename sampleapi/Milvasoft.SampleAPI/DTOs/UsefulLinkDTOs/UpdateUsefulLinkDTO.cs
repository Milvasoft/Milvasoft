using System;

namespace Milvasoft.SampleAPI.DTOs.UsefulLinkDTOs
{
    /// <summary>
    /// UpdateUsefulLinkDTO for update useful link operations.
    /// </summary>
    public class UpdateUsefulLinkDTO
    {
        public Guid Id { get; set; }
        /// <summary>
        /// Tittle of link.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Description of link.
        /// </summary>
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
