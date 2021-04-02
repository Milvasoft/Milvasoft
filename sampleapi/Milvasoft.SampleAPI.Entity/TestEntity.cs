using Milvasoft.Helpers.MultiTenancy.EntityBase;
using System;

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
