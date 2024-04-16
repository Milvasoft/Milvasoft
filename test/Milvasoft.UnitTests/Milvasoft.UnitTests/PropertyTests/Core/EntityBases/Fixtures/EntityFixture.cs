using Milvasoft.Core.EntityBases.Concrete;
using Milvasoft.Core.EntityBases.Concrete.Auditing;

namespace Milvasoft.UnitTests.PropertyTests.Core.EntityBases.Fixtures;

public class EntityFixture : FullAuditableEntity<int>
{
}

public class EntityWithoutUserFixture : FullAuditableEntityWithoutUser<int>
{
}

public class EntityBaseFixture : EntityBase<int>
{
}
