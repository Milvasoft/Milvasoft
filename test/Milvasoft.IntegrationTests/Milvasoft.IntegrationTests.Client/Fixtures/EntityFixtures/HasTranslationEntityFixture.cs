using Milvasoft.Core.EntityBases.Concrete;
using Milvasoft.Core.MultiLanguage.EntityBases.Abstract;
using Milvasoft.Core.MultiLanguage.EntityBases.Concrete;
using System.ComponentModel.DataAnnotations.Schema;

namespace Milvasoft.IntegrationTests.Client.Fixtures.EntityFixtures;

public class HasTranslationEntityFixture : HasTranslationEntity<TranslationEntityFixture>
{
    public int Priority { get; set; }
}

public class HasJsonTranslationEntityFixture : BaseEntity<int>, IHasTranslation<JsonTranslationEntityFixture>
{
    public int Priority { get; set; }

    [Column(TypeName = "jsonb")]
    public List<JsonTranslationEntityFixture> Translations { get; set; }
}
