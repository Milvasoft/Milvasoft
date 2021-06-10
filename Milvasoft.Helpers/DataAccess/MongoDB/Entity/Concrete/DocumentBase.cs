using Milvasoft.Helpers.DataAccess.MongoDB.Entity.Abstract;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace Milvasoft.Helpers.DataAccess.MongoDB.Entity.Concrete
{
    /// <summary>
    /// Base entity for most obk entities.
    /// </summary>
    public abstract class DocumentBase : IDocumentBase
    {
        /// <summary>
        /// Unique key of entity.
        /// </summary>
        [BsonId]
        [BsonRepresentation(BsonType.String)]
        public ObjectId Id { get; set; } = ObjectId.GenerateNewId();

        /// <summary>
        /// Last modification date of entity.
        /// </summary>
        public DateTime? LastModificationDate { get; set; }

        /// <summary>
        /// Creation date of entity.
        /// </summary>
        //public DateTime CreationDate { get => Id.CreationTime; set { } }
        public DateTime CreationDate { get; set; }
    }
}
