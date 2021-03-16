using Microsoft.AspNetCore.Identity;
using Milvasoft.Helpers.DataAccess.Abstract.Entity;
using System;

namespace Milvasoft.SampleAPI.Entity
{
    public class AppRole : IdentityRole<Guid>, IFullAuditable<Guid>
    {
        public string PhotoPath { get; set; }

        public bool IsConfirmedUser { get; set; }

        public bool IsMentor { get; set; }

        public DateTime? LastModificationDate { get; set; }

        public DateTime CreationDate { get; set; }

        public Guid? CreatorUserId { get; set; }

        public Guid? LastModifierUserId { get; set; }

        public DateTime? DeletionDate { get; set; }

        public bool IsDeleted { get; set; }

        public Guid? DeleterUserId { get; set; }


        public virtual Mentor Mentor { get; set; }

        public virtual Student Student { get; set; }
    }
}
