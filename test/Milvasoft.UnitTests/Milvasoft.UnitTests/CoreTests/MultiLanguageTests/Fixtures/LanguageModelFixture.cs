using Milvasoft.Core.MultiLanguage.EntityBases.Abstract;

namespace Milvasoft.UnitTests.CoreTests.MultiLanguageTests.Fixtures;
public class LanguageModelFixture : ILanguage
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Code { get; set; }
    public bool Supported { get; set; }
    public bool IsDefault { get; set; }
}
