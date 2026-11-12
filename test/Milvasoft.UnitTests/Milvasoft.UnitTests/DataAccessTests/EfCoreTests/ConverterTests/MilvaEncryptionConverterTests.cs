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
    public void EncryptedJsonValueConverter_WithDictionaryProperties_ShouldRoundTripAndEncryptNestedValues()
    {
        // Arrange
        var crypto = new Moq.Mock<Milvasoft.Cryptography.Abstract.IMilvaCryptographyProvider>();
        crypto.Setup(c => c.Encrypt(Moq.It.IsAny<string>())).Returns<string>(v => $"E({v})");
        crypto.Setup(c => c.Decrypt(Moq.It.IsAny<string>())).Returns<string>(v => v[2..^1]);

        var converter = new EncryptedJsonValueConverter<DictionaryHolder>(crypto.Object);

        var model = new DictionaryHolder
        {
            Secret = "secret",
            Labels = new() { ["tr"] = "etiket", ["en"] = "label" },
            Items = new() { [1] = new SecretItem { Value = "first" } }
        };

        // Act
        var json = (string)converter.ConvertToProvider(model);
        var result = (DictionaryHolder)converter.ConvertFromProvider(json);

        // Assert
        json.Should().NotContain("\"secret\"").And.NotContain("\"first\"").And.Contain("etiket");
        result.Secret.Should().Be("secret");
        result.Labels.Should().BeEquivalentTo(model.Labels);
        result.Items[1].Value.Should().Be("first");
    }

    public class DictionaryHolder
    {
        [Milvasoft.Attributes.Annotations.Encrypted]
        public string Secret { get; set; }
        public Dictionary<string, string> Labels { get; set; }
        public Dictionary<int, SecretItem> Items { get; set; }
    }

    public class SecretItem
    {
        [Milvasoft.Attributes.Annotations.Encrypted]
        public string Value { get; set; }
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
