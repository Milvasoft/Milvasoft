using Milvasoft.Helpers.DataAccess.Concrete.Entity;
using Milvasoft.SampleAPI.DTOs.ProfessionDTOs;
using Milvasoft.SampleAPI.Entity;
using Milvasoft.SampleAPI.Utils.Attributes.ValidationAttributes;
using System;

namespace Milvasoft.SampleAPI.DTOs
{
    /// <summary>
    /// Useful links.
    /// </summary>
    public class UsefulLinkDTO : AuditableEntity<AppUser, Guid, Guid>
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
        [OValidateId]
        public Guid ProfessionId { get; set; }

        /// <summary>
        /// Profession of link.
        /// </summary>
        public virtual ProfessionDTO Profession { get; set; }
    }
}
