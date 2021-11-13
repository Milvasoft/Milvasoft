using Milvasoft.Helpers.DataAccess.EfCore.Concrete.Entity;
using System;

namespace Milvasoft.SampleAPI.Entity
{

    /// <summary>
    /// Mentor and profession relationship.
    /// </summary>
    public class MentorProfession : AuditableEntity<AppUser, Guid, Guid>
    {

        /// <summary>
        /// Id of mentor.
        /// </summary>
        public Guid MentorId { get; set; }

        /// <summary>
        /// Mentor.
        /// </summary>
        public virtual Mentor Mentor { get; set; }



        /// <summary>
        /// Id of profession.
        /// </summary>
        public Guid ProfessionId { get; set; }

        /// <summary>
        /// Profession.
        /// </summary>
        public virtual Profession Profession { get; set; }
    }
}
