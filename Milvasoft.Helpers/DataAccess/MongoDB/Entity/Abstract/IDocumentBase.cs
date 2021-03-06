﻿using Milvasoft.Helpers.DataAccess.Abstract.Entity;
using MongoDB.Bson;

namespace Milvasoft.Helpers.DataAccess.MongoDB.Entity.Abstract
{
    /// <summary>
    /// Base interface for most obk entities.
    /// </summary>
    public interface IDocumentBase : IAuditable<ObjectId>
    {
    }
}
