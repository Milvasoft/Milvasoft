using Milvasoft.Core.MultiLanguage.EntityBases.Concrete;

namespace Milvasoft.IntegrationTests.Client.Fixtures.EntityFixtures;
public class HasTranslationEntityFixture : HasTranslationEntity<TranslationEntityFixture>
{
    public int Priority { get; set; }
}
