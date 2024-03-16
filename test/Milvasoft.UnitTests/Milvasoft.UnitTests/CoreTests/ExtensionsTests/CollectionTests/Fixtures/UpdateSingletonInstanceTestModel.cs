namespace Milvasoft.UnitTests.CoreTests.ExtensionsTests.CollectionTests.Fixtures;

public interface IUpdateSingletonInstanceTestModel 
{
    public string Name { get; set; }
    public byte Order { get; set; }
}

public class UpdateSingletonInstanceTestModel : IUpdateSingletonInstanceTestModel
{
    public string Name { get; set; }
    public byte Order { get; set; }
}
