﻿using Milvasoft.Helpers.DataAccess.EfCore.Concrete.Entity;
using System;

namespace Milvasoft.SampleAPI.Entity
{

    /// <summary>
    /// Base entity of all entities.
    /// </summary>
    public abstract class EducationEntityBase : FullAuditableEntity<AppUser, Guid, Guid>
    {
    }
}
