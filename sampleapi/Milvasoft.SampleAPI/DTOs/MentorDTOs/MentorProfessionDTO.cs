using Milvasoft.Helpers.DataAccess.Concrete.Entity;
using Milvasoft.SampleAPI.DTOs.ProfessionDTOs;
using Milvasoft.SampleAPI.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Milvasoft.SampleAPI.DTOs.MentorDTOs
{

    /// <summary>
    /// Mentor and profession relationship.
    /// </summary>
    public class MentorProfessionDTO : AuditableEntity<AppUser, Guid, Guid>
    {

        /// <summary>
        /// Id of mentor.
        /// </summary>
        public Guid MentorId { get; set; }

        /// <summary>
        /// Mentor.
        /// </summary>
        public virtual MentorDTO Mentor { get; set; }



        /// <summary>
        /// Id of profession.
        /// </summary>
        public Guid ProfessionId { get; set; }

        /// <summary>
        /// Profession.
        /// </summary>
        public virtual ProfessionDTO Profession { get; set; }
    }
}
