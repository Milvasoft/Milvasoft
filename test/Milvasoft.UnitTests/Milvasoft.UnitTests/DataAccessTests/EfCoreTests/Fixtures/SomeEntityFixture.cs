using Milvasoft.Attributes.Annotations;
using Milvasoft.Core.EntityBases.Concrete;
using Milvasoft.Core.EntityBases.Concrete.Auditing;
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

public class SomeFullAuditableEntityFixture : FullAuditableEntity<int>
{
    public string SomeStringProp { get; set; }
    public DateTime SomeDateProp { get; set; }
    public decimal SomeDecimalProp { get; set; }
}

public class PolicyTestEntityFixture : FullAuditableEntity<int>
{
    public string Name { get; set; }
    public string PasswordHash { get; set; }

    [Encrypted]
    public string EncryptedValue { get; set; }

    [Filterable(true)]
    public int Rank { get; set; }
}
