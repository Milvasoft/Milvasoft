using Milvasoft.Core.EntityBases.Concrete;
using Milvasoft.Types.Structs;

namespace Milvasoft.UnitTests.CoreTests.HelperTests.CommonTests.Fixtures;

public class UpdatedPropsTestDto : BaseDto<int>
{
    public UpdateProperty<string>? Name { get; set; }
    public UpdateProperty<decimal>? Price { get; set; }
    public UpdateProperty<int>? Priority { get; set; }
    public UpdateProperty<DateTime>? UpdateDate { get; set; }
    public UpdateProperty<byte>? Type { get; set; }
}

public class UpdatedPropsTestInvalidDto : BaseDto<int>
{
    public string Name { get; set; }
    public decimal Price { get; set; }
}
