using Milvasoft.Core.EntityBases.Concrete;
using Milvasoft.Core.EntityBases.Concrete.Auditing;
using Milvasoft.Core.EntityBases.Concrete.Auditing.Dto;
using Milvasoft.Core.MultiLanguage.EntityBases.Concrete;
using System.ComponentModel.DataAnnotations;

namespace Milvasoft.UnitTests.DataAccessTests.EfCoreTests.Fixtures;

public class SomeEntityFixture
{
    [Key]
    public int Id { get; set; }
    public string SomeStringProp { get; set; }
    public DateTime SomeDateProp { get; set; }
    public decimal SomeDecimalProp { get; set; }
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

public class SomeCreationAuditableEntityFixture : CreationAuditableDto<int>
{
    public string SomeStringProp { get; set; }
    public DateTime SomeDateProp { get; set; }
    public decimal SomeDecimalProp { get; set; }
}

public class SomeAuditableEntityFixture : AuditableEntity<int>
{
    public string SomeStringProp { get; set; }
    public DateTime SomeDateProp { get; set; }
    public decimal SomeDecimalProp { get; set; }
}

public class SomeFullAuditableEntityFixture : FullAuditableEntity<int>
{
    public string SomeStringProp { get; set; }
    public DateTime SomeDateProp { get; set; }
    public decimal SomeDecimalProp { get; set; }
}

public class SomeHasTranslationEntityFixture : HasTranslationEntity<SomeTranslationEntityFixture>
{
    public DateTime SomeDateProp { get; set; }
    public decimal SomeDecimalProp { get; set; }
}

public class SomeTranslationEntityFixture : TranslationEntity<SomeHasTranslationEntityFixture>
{
    public string SomeTranslation { get; set; }
}