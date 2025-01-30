using Milvasoft.Core.MultiLanguage.EntityBases.Concrete;

namespace Milvasoft.IntegrationTests.Client.Fixtures.EntityFixtures;

public class TranslationEntityFixture : TranslationEntity<HasTranslationEntityFixture>
{
    public string Name { get; set; }
    public string Description { get; set; }
}

public class JsonTranslationEntityFixture : TranslationEntity<HasJsonTranslationEntityFixture>
{
    public string Name { get; set; }
    public string Description { get; set; }
}
