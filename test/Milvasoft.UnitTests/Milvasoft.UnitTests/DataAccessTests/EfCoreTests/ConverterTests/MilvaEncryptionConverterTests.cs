using FluentAssertions;
using Milvasoft.Cryptography.Builder;
using Milvasoft.Cryptography.Concrete;
using Milvasoft.DataAccess.EfCore.Utils.Converters;

namespace Milvasoft.UnitTests.DataAccessTests.EfCoreTests.ConverterTests;

public class MilvaEncryptionConverterTests
{
    [Fact]
    public void MilvaEncryptionConverter_Constructor_ShouldInitializeCorrectly()
    {
        // Arrange
        var options = new MilvaCryptographyOptions
        {
            Key = "A=YI=+B_LfoK%V4r"
        };
        var provider = new MilvaCryptographyProvider(options, new Microsoft.IO.RecyclableMemoryStreamManager());

        // Act
        var converter = new MilvaEncryptionConverter(provider);

        // Assert
        converter.Should().NotBeNull();
    }

    [Fact]
    public void MilvaObjectIdStringConverter_Constructor_ShouldInitializeCorrectly()
    {
        // Arrange & Act
        var converter = new MilvaObjectIdStringConverter();

        // Assert
        converter.Should().NotBeNull();
    }
}
