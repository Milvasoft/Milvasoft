using Milvasoft.Core.EntityBases.Abstract.Auditing;

namespace Milvasoft.UnitTests.CoreTests.HelperTests.CommonTests.Fixtures;

internal class SoftDeletableTestEntity : ISoftDeletable
{
    public DateTime? DeletionDate { get; set; }

    public bool IsDeleted { get; set; }
}
