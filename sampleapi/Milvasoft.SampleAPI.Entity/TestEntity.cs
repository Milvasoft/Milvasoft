using Milvasoft.Helpers.MultiTenancy.EntityBase;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Milvasoft.SampleAPI.Entity
{
    /// <summary>
    /// Test entity for tenant.
    /// </summary>
    public class TestEntity : MilvaTenant<AppUser, Guid>
    {
        protected TestEntity() : base()
        {
        }

        public TestEntity(string tenancyName, int branchNo) : base(tenancyName, branchNo)
        {
        }

    }
}
