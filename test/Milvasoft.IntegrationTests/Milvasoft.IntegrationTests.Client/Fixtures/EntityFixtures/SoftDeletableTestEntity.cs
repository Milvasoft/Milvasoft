using Milvasoft.Core.EntityBases.Abstract.Auditing;

namespace Milvasoft.IntegrationTests.Client.Fixtures.EntityFixtures;

internal class SoftDeletableTestEntity : ISoftDeletable
{
    public DateTime? DeletionDate { get; set; }

    public bool IsDeleted { get; set; }
}
