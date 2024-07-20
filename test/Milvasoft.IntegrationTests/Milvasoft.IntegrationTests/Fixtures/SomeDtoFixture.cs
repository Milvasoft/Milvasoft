﻿using Milvasoft.Core.EntityBases.Concrete;
using Milvasoft.Types.Structs;

namespace Milvasoft.UnitTests.DataAccessTests.EfCoreTests.Fixtures;

public class DtoFixture : BaseDto<int>
{
    public UpdateProperty<string>? SomeStringProp { get; set; }
    public UpdateProperty<decimal>? SomeDecimalProp { get; set; }
    public UpdateProperty<DateTime>? SomeDateProp { get; set; }
}