using Milvasoft.Core.EntityBases.Concrete;
using Milvasoft.Core.EntityBases.Concrete.Auditing.Dto;

namespace Milvasoft.UnitTests.PropertyTests.Core.EntityBases.Fixtures;

public class DtoFixture : FullAuditableDto<int>
{
}

public class DtoWithoutUserFixture : FullAuditableDtoWithoutUser<int>
{
}

public class DtoBaseFixture : DtoBase<int>
{
}
