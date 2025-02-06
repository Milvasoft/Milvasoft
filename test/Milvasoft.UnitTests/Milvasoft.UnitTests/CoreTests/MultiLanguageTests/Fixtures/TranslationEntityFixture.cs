using Milvasoft.Core.EntityBases.Concrete.Auditing;
using Milvasoft.Core.MultiLanguage.EntityBases.Abstract;
using Milvasoft.Core.MultiLanguage.EntityBases.Concrete;
using System.ComponentModel.DataAnnotations.Schema;

namespace Milvasoft.UnitTests.CoreTests.MultiLanguageTests.Fixtures;

public class TranslationEntityFixture : TranslationEntity<HasTranslationEntityFixture>
{
    public string Name { get; set; }
    public string Description { get; set; }
}

public class FullAuditableTranslationEntityFixture : FullAuditableEntity<int>, ITranslationEntityWithIntKey<FullAuditableHasTranslationEntityFixture>
{
    public string Name { get; set; }
    public string Description { get; set; }
    public int EntityId { get; set; }
    public int LanguageId { get; set; }

    [NotMapped]
    public FullAuditableHasTranslationEntityFixture Entity { get; set; }
}
