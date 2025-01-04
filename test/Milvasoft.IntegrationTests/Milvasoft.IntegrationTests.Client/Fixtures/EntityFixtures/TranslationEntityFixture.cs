using Milvasoft.Core.MultiLanguage.EntityBases.Concrete;

namespace Milvasoft.IntegrationTests.Client.Fixtures.EntityFixtures;

public class TranslationEntityFixture : TranslationEntity<HasTranslationEntityFixture>
{
    public string Name { get; set; }
    public string Description { get; set; }
}
