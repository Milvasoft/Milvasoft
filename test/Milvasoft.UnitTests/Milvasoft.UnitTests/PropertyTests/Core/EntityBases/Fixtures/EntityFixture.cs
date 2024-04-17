using Milvasoft.Core.EntityBases.Concrete;
using Milvasoft.Core.EntityBases.Concrete.Auditing;
using Milvasoft.Core.MultiLanguage.EntityBases.Concrete;

namespace Milvasoft.UnitTests.PropertyTests.Core.EntityBases.Fixtures;

public class EntityFixture : FullAuditableEntity<int>
{
}

public class EntityWithoutUserFixture : FullAuditableEntityWithoutUser<int>
{
}

public class EntityBaseFixture : EntityBase<int>
{
}

public class LanguageEntityFixture : LanguageEntity
{
    public override object GetUniqueIdentifier() => Id;
}

public class HasTranslationEntityFixture : HasTranslationEntity<TranslationEntityFixture>
{
}

public class TranslationEntityFixture : TranslationEntity<HasTranslationEntityFixture>
{
}
