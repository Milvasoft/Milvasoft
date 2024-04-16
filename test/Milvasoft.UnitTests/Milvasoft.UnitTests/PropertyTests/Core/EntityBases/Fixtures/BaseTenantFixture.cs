using Milvasoft.Core.EntityBases.MultiTenancy;

namespace Milvasoft.UnitTests.PropertyTests.Core.EntityBases.Fixtures;

[System.Diagnostics.CodeAnalysis.SuppressMessage("Major Code Smell", "S110:Inheritance tree of classes should not be too deep", Justification = "<Pending>")]
public class BaseTenantFixture : MilvaBaseTenant<int>
{
}

[System.Diagnostics.CodeAnalysis.SuppressMessage("Major Code Smell", "S110:Inheritance tree of classes should not be too deep", Justification = "<Pending>")]
public class MilvaTenantFixture : MilvaTenant
{
    public MilvaTenantFixture() : base()
    {

    }

    public MilvaTenantFixture(string tenancyName, int branchNo) : base(tenancyName, branchNo)
    {

    }
}

