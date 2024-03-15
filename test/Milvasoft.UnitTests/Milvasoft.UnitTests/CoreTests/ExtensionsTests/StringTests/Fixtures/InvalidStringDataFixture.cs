using System.Collections;

namespace Milvasoft.UnitTests.CoreTests.ExtensionsTests.StringTests.Fixtures;

internal class InvalidStringDataFixture : IEnumerable<object[]>
{
    /// <summary>
    /// input,  culture code, expected
    /// </summary>
    /// <returns></returns>
    public IEnumerator<object[]> GetEnumerator()
    {
        yield return new object[] { "", "tr-TR", "" };
        yield return new object[] { "", "en-US", "" };
        yield return new object[] { null, "tr-TR", null };
        yield return new object[] { null, "en-US", null };
        yield return new object[] { " ", "tr-TR", " " };
        yield return new object[] { " ", "en-US", " " };
        yield return new object[] { "1234", "tr-TR", "1234" };
        yield return new object[] { "1234", "en-US", "1234" };
        yield return new object[] { "!Hello", "tr-TR", "!Hello" };
        yield return new object[] { "!Hello", "en-US", "!Hello" };
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
