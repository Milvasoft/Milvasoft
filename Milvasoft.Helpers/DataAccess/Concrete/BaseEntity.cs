using Milvasoft.Helpers.DataAccess.Abstract;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Milvasoft.Helpers.DataAccess.Concrete
{
    /// <summary>
    /// <para> Base entity for all of erasable entities. All erasable entities have this values. </para>
    /// </summary>
    public abstract class BaseEntity<TKey> : IBaseEntity<TKey> where TKey : IEquatable<TKey>
    {
        /// <summary>
        /// <para> Unique database key. Represents primary key.</para>
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public TKey Id { get; set; }

        /// <summary>
        /// <para> Date added to database for entity.</para>
        /// </summary>
        public DateTime InsertedDate { get; set; } = DateTime.Now;

        /// <summary>
        /// <para> Update date in database for entity.</para>
        /// </summary>
        public DateTime? LastUpdatedDate { get; set; }

    }
}
