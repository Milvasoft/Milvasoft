using Milvasoft.Core.EntityBases.Concrete.Auditing;
using Milvasoft.Core.MultiLanguage.EntityBases.Abstract;
using Milvasoft.Core.MultiLanguage.EntityBases.Concrete;

namespace Milvasoft.UnitTests.CoreTests.MultiLanguageTests.Fixtures;
public class HasTranslationEntityFixture : HasTranslationEntity<TranslationEntityFixture>
{
    public int Priority { get; set; }
}

public class FullAuditableHasTranslationEntityFixture : FullAuditableEntity<int>, IHasTranslation<FullAuditableTranslationEntityFixture>
{
    public int Priority { get; set; }
    public List<FullAuditableTranslationEntityFixture> Translations { get; set; }
}
