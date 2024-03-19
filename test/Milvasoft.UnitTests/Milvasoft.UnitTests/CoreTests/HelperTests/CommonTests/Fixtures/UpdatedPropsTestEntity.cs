using Milvasoft.Core.EntityBases.Concrete;

namespace Milvasoft.UnitTests.CoreTests.HelperTests.CommonTests.Fixtures;

internal class UpdatedPropsTestEntity : BaseEntity<int>
{
    public string Name { get; set; }
    public decimal? Price { get; set; }
    public int Priority { get; set; }
}
