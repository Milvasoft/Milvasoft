using Microsoft.AspNetCore.Identity;
using Milvasoft.Helpers.DataAccess.Abstract.Entity;
using System;

namespace Milvasoft.SampleAPI.Entity.Identity
{
    public class MilvaRole : IdentityRole<Guid>, IFullAuditable<Guid>
    {
        public DateTime? LastModificationDate { get; set; }

        public DateTime CreationDate { get; set; }
        public DateTime? DeletionDate { get; set; }
        public bool IsDeleted { get; set; }
    }
}
