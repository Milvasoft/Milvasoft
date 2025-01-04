using Milvasoft.Core.MultiLanguage.EntityBases.Concrete;

namespace Milvasoft.IntegrationTests.Client.Fixtures.DtoFixtures;

public class TranslationDtoFixture : TranslationEntity<HasTranslationDtoFixture>
{
    public string Name { get; set; }
    public string Description { get; set; }
}
