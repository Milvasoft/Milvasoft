using Microsoft.EntityFrameworkCore;
using Milvasoft.Attributes.Annotations;
using Milvasoft.Core.EntityBases.Concrete;
using Milvasoft.Core.EntityBases.Concrete.Auditing;
using Milvasoft.Core.EntityBases.MultiTenancy;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Milvasoft.IntegrationTests.Client.Fixtures.EntityFixtures;

public class SomeEntityFixture
{
    [Key]
    public int Id { get; set; }
    public string SomeStringProp { get; set; }
    public DateTime SomeDateProp { get; set; }
    public decimal SomeDecimalProp { get; set; }
    public virtual List<SomeRelatedEntityFixture> RelatedEntities { get; set; }
    public virtual List<SomeManyToOneFullAuditableEntityFixture> ManyToOneEntities { get; set; }
}

public class SomeRelatedEntityFixture
{
    [Key]
    public int Id { get; set; }
    public string SomeStringProp { get; set; }
    public DateTime SomeDateProp { get; set; }
    public decimal SomeDecimalProp { get; set; }

    [ForeignKey(nameof(Entity))]
    public int EntityId { get; set; }
    public virtual SomeEntityFixture Entity { get; set; }
}

public class SomeBaseEntityFixture : BaseEntity<int>
{
    public string SomeStringProp { get; set; }
    public DateTime SomeDateProp { get; set; }
    public decimal SomeDecimalProp { get; set; }
    public virtual DateTime? CreationDate { get; set; }
    public virtual string CreatorUserName { get; set; }
    public virtual DateTime? LastModificationDate { get; set; }
    public virtual string LastModifierUserName { get; set; }
    public virtual DateTime? DeletionDate { get; set; }
    public virtual string DeleterUserName { get; set; }
    public virtual bool IsDeleted { get; set; }
}

public class SomeFullAuditableEntityFixture : FullAuditableEntity<int>
{
    public string SomeStringProp { get; set; }
    public DateTime SomeDateProp { get; set; }
    public DateTimeOffset SomeDateTimeOffsetProp { get; set; }
    public decimal SomeDecimalProp { get; set; }

    [CascadeOnDelete]
    public virtual List<SomeManyToOneFullAuditableEntityFixture> ManyToOneEntities { get; set; }
}

public class AnotherFullAuditableEntityFixture : FullAuditableEntity<int>
{
    public string SomeStringProp { get; set; }
    public DateTime SomeDateProp { get; set; }
    public DateTimeOffset SomeDateTimeOffsetProp { get; set; }
    public decimal SomeDecimalProp { get; set; }
    public virtual List<SomeManyToOneFullAuditableEntityFixture> ManyToOneEntities { get; set; }
}

public class SomeManyToOneFullAuditableEntityFixture : FullAuditableEntity<int>
{
    public string SomeStringProp { get; set; }
    public DateTimeOffset SomeDateProp { get; set; }
    public decimal SomeDecimalProp { get; set; }

    [ForeignKey(nameof(SomeFullAuditableEntity))]
    public int SomeFullAuditableEntityId { get; set; }

    public virtual SomeFullAuditableEntityFixture SomeFullAuditableEntity { get; set; }

    [ForeignKey(nameof(SomeEntity))]
    public int SomeEntityId { get; set; }

    public virtual SomeEntityFixture SomeEntity { get; set; }
}

public class SomeManyToManyEntityFixture : FullAuditableEntity<int>
{
    [ForeignKey(nameof(SomeEntity))]
    public int SomeEntityId { get; set; }

    public virtual SomeFullAuditableEntityFixture SomeEntity { get; set; }

    [ForeignKey(nameof(AnotherEntity))]
    public int AnotherEntityId { get; set; }

    public virtual AnotherFullAuditableEntityFixture AnotherEntity { get; set; }
}

public class SomeCircularReferenceFullAuditableEntityFixture : FullAuditableEntity<int>
{
    public string SomeStringProp { get; set; }
    public DateTimeOffset SomeDateProp { get; set; }
    public decimal SomeDecimalProp { get; set; }

    [ForeignKey(nameof(SomeEntity))]
    public int? SomeEntityId { get; set; }

    public virtual SomeCircularReferenceFullAuditableEntityFixture SomeEntity { get; set; }
}

public class SomeModelBuilderTestEntityFixture : FullAuditableEntity<int>
{
    public TenantId? TenantId { get; set; }
    public string SomeEncryptedStringProp { get; set; }

    [Encrypted]
    public string SomeEncryptedStringWithAttributeProp { get; set; }
    public DateTimeOffset? SomeNullableDateTimeOffsetProp { get; set; }
    public DateTimeOffset SomeDateTimeOffsetProp { get; set; }
    public DateTime? SomeNullableDateProp { get; set; }
    public DateTime SomeDateProp { get; set; }

    [DefaultValue(1)]
    public int SomeIntProp { get; set; }

    [DecimalPrecision]
    public decimal? SomeDecimalProp { get; set; }
}

[Keyless]
public class SomeModelBuilderTestKeylessEntityFixture
{
    public string SomeEncryptedStringProp { get; set; }
    public string SomeEncryptedStringWithAttributeProp { get; set; }
    public DateTimeOffset? SomeNullableDateTimeOffsetProp { get; set; }
    public DateTimeOffset SomeDateTimeOffsetProp { get; set; }
    public DateTime? SomeNullableDateProp { get; set; }
    public DateTime SomeDateProp { get; set; }
    public int SomeIntProp { get; set; }
    public decimal? SomeDecimalProp { get; set; }
}

public class SomeLogEntity : LogEntityBase<int>
{
}