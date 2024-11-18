using Milvasoft.Core.EntityBases.MultiTenancy;

namespace Milvasoft.UnitTests.PropertyTests.Core.EntityBases.Fixtures;

public class BaseTenantFixture : MilvaBaseTenant<int>;

public class MilvaTenantFixture : MilvaTenant
{
    public MilvaTenantFixture() : base()
    {

    }

    public MilvaTenantFixture(string tenancyName, int branchNo) : base(tenancyName, branchNo)
    {

    }
}

