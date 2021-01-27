using Milvasoft.Helpers.DataAccess.Abstract;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Milvasoft.Helpers.DataAccess.Concrete
{
    /// <summary>
    /// Base entity for all of erasable entities. All erasable entities have this values. 
    /// </summary>
    public abstract class BaseEntity<TKey> : IBaseEntity<TKey> where TKey : IEquatable<TKey>
    {
        /// <summary>
        /// Unique database key. Represents primary key.
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public TKey Id { get; set; }

        /// <summary>
        /// Date added to database for entity.
        /// </summary>
        public DateTime InsertedDate { get; set; } = DateTime.Now;

        /// <summary>
        /// Update date in database for entity.
        /// </summary>
        public DateTime? LastUpdatedDate { get; set; }

    }
}
