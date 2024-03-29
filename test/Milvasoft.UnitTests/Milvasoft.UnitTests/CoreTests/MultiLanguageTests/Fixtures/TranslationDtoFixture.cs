using Milvasoft.Core.MultiLanguage.EntityBases.Concrete;

namespace Milvasoft.UnitTests.CoreTests.MultiLanguageTests.Fixtures;

public class TranslationDtoFixture : TranslationEntity<HasTranslationDtoFixture>
{
    public string Name { get; set; }
    public string Description { get; set; }
}
