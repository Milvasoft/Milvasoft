﻿using Milvasoft.Core.MultiLanguage.EntityBases.Concrete;

namespace Milvasoft.UnitTests.CoreTests.MultiLanguageTests.Fixtures;

public class TranslationEntityFixture : TranslationEntity<HasTranslationEntityFixture>
{
    public string Name { get; set; }
    public string Description { get; set; }
}
