using Milvasoft.Helpers.DataAccess.Concrete.Entity;
using Milvasoft.SampleAPI.DTOs.ProfessionDTOs;
using Milvasoft.SampleAPI.Entity;
using Milvasoft.SampleAPI.Utils.Attributes.ValidationAttributes;
using Milvasoft.SampleAPI.Utils.Swagger;
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
        [OValidateId]
        public Guid MentorId { get; set; }

        /// <summary>
        /// Mentor.
        /// </summary>
        [SwaggerExclude]
        public virtual MentorDTO Mentor { get; set; }



        /// <summary>
        /// Id of profession.
        /// </summary>
        [OValidateId]
        public Guid ProfessionId { get; set; }

        /// <summary>
        /// Profession.
        /// </summary>
        [SwaggerExclude]
        public virtual ProfessionDTO Profession { get; set; }
    }
}
