namespace Milvasoft.UnitTests.CoreTests.HelperTests.PropertyTests.Fixtures;

public class PropertyExistsTestModelFixture : IComparable
{
    public byte Poco { get; set; }

    public int CompareTo(object obj)
    {
        if (obj == null)
            return 1;

        if (obj is PropertyExistsTestModelFixture otherModel)
        {
            // Compare the Poco property
            return Poco.CompareTo(otherModel.Poco);
        }

        throw new ArgumentException($"Object type is not a {nameof(PropertyExistsTestModelFixture)}");
    }
}
