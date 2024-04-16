namespace Milvasoft.UnitTests.CoreTests.UtilTests.ConverterTests.Fixtures;

public class GenericClassImplementsGenericInterfaceFixture<T> : IGenericInterfaceFixture<T>
{
    public string Name { get; set; }
    public T Class { get; set; }
}
