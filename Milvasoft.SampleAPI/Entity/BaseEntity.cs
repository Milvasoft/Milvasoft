using Milvasoft.Helpers.DataAccess.Abstract.Entity;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Milvasoft.SampleAPI.Entity
{
    public abstract class BaseEntity : IBaseEntity<Guid>
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        public DateTime CreationDate { get; set; }

        public DateTime? LastModificationDate { get; set; }
    }
}
