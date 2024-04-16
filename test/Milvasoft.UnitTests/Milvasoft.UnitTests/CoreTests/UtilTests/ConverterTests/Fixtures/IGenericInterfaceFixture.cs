namespace Milvasoft.UnitTests.CoreTests.UtilTests.ConverterTests.Fixtures;

public interface IGenericInterfaceFixture<T>
{
    public string Name { get; set; }
    public T Class { get; set; }
}
