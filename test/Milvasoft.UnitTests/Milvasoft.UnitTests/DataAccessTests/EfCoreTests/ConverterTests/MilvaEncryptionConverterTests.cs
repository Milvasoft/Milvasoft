using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Milvasoft.Core.Abstractions;
using Milvasoft.Cryptography.Builder;
using Milvasoft.Cryptography.Concrete;
using Milvasoft.DataAccess.EfCore.DbContextBase;
using Milvasoft.DataAccess.EfCore.Utils.Converters;
using Moq;
using System.Security.Cryptography;

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
    public void MilvaEncryptionConverter_WithInputStartingWithPrefix_ShouldEncryptItAndRoundTrip()
    {
        // Arrange
        var converter = new MilvaEncryptionConverter(CreateProvider());
        const string userInput = "enc::not-a-cipher-text";

        // Act
        var stored = (string)converter.ConvertToProvider(userInput);
        var read = (string)converter.ConvertFromProvider(stored);

        // Assert
        stored.Should().StartWith(EncryptedValue.Prefix).And.NotBe(userInput);
        read.Should().Be(userInput);
    }

    [Fact]
    public void MilvaEncryptionConverter_WithValueWithoutPrefix_ShouldReturnItAsItIs()
        => new MilvaEncryptionConverter(CreateProvider()).ConvertFromProvider("plain legacy value").Should().Be("plain legacy value");

    [Fact]
    public void MilvaEncryptionConverter_WithUndecryptableValueAndWithoutLogger_ShouldThrow()
    {
        // Arrange
        var converter = new MilvaEncryptionConverter(CreateProvider());

        // Act
        var act = () => converter.ConvertFromProvider("enc::corrupted-cipher-text");

        // Assert
        act.Should().Throw<CryptographicException>();
    }

    [Fact]
    public void MilvaEncryptionConverter_WithUndecryptableValueAndLogger_ShouldLogAndReturnStoredValue()
    {
        // Arrange
        var logger = new Mock<IMilvaLogger>();
        var converter = new MilvaEncryptionConverter(CreateProvider(), logger: logger.Object);

        // Act
        var read = converter.ConvertFromProvider("enc::corrupted-cipher-text");

        // Assert
        read.Should().Be("enc::corrupted-cipher-text");
        logger.Verify(l => l.Error(It.IsAny<Exception>(), It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public void EncryptedJsonValueConverter_WithInputStartingWithPrefix_ShouldEncryptItAndRoundTrip()
    {
        // Arrange
        var converter = new EncryptedJsonValueConverter<SecretItem>(CreateProvider());
        var model = new SecretItem { Value = "enc::not-a-cipher-text" };

        // Act
        var json = (string)converter.ConvertToProvider(model);
        var read = (SecretItem)converter.ConvertFromProvider(json);

        // Assert
        json.Should().NotContain("not-a-cipher-text");
        read.Value.Should().Be(model.Value);
    }

    [Fact]
    public void EncryptedJsonValueConverter_WithUndecryptableValueAndWithoutLogger_ShouldThrow()
    {
        // Arrange
        var converter = new EncryptedJsonValueConverter<SecretItem>(CreateProvider());

        // Act
        var act = () => converter.ConvertFromProvider("""{"Value":"enc::corrupted-cipher-text"}""");

        // Assert
        act.Should().Throw<CryptographicException>();
    }

    [Fact]
    public void UseEncryption_ShouldNotEncryptKeysForeignKeysDiscriminatorsConcurrencyTokensAndIndexes()
    {
        // Arrange
        using var context = new EncryptionModelContext();

        // Act
        var model = context.Model;

        // Assert
        var principal = model.FindEntityType(typeof(EncryptionPrincipal));
        var dependent = model.FindEntityType(typeof(EncryptionDependent));
        var derived = model.FindEntityType(typeof(EncryptionDerivedDependent));

        IsEncrypted(principal, nameof(EncryptionPrincipal.Id)).Should().BeFalse();
        IsEncrypted(principal, nameof(EncryptionPrincipal.Code)).Should().BeFalse();
        IsEncrypted(principal, nameof(EncryptionPrincipal.Indexed)).Should().BeFalse();
        IsEncrypted(principal, nameof(EncryptionPrincipal.Version)).Should().BeFalse();
        IsEncrypted(principal, nameof(EncryptionPrincipal.Secret)).Should().BeTrue();

        IsEncrypted(dependent, nameof(EncryptionDependent.PrincipalCode)).Should().BeFalse();
        IsEncrypted(dependent, "Discriminator").Should().BeFalse();
        IsEncrypted(dependent, nameof(EncryptionDependent.Note)).Should().BeTrue();
        IsEncrypted(derived, nameof(EncryptionDerivedDependent.Extra)).Should().BeTrue();

        static bool IsEncrypted(IEntityType entityType, string propertyName) => entityType.FindProperty(propertyName).GetValueConverter() is MilvaEncryptionConverter;
    }

    private static MilvaCryptographyProvider CreateProvider() => new(new MilvaCryptographyOptions { Key = "A=YI=+B_LfoK%V4r" }, new Microsoft.IO.RecyclableMemoryStreamManager());

    public class EncryptionPrincipal
    {
        public string Id { get; set; }
        public string Code { get; set; }
        public string Indexed { get; set; }
        public string Version { get; set; }
        public string Secret { get; set; }
    }

    public class EncryptionDependent
    {
        public int Id { get; set; }
        public string PrincipalCode { get; set; }
        public EncryptionPrincipal Principal { get; set; }
        public string Note { get; set; }
    }

    public class EncryptionDerivedDependent : EncryptionDependent
    {
        public string Extra { get; set; }
    }

    private sealed class EncryptionModelContext : DbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder.UseInMemoryDatabase(nameof(EncryptionModelContext));

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<EncryptionPrincipal>(b =>
            {
                b.HasKey(e => e.Id);
                b.HasAlternateKey(e => e.Code);
                b.HasIndex(e => e.Indexed);
                b.Property(e => e.Version).IsConcurrencyToken();
            });

            modelBuilder.Entity<EncryptionDependent>(b =>
            {
                b.HasOne(e => e.Principal).WithMany().HasForeignKey(e => e.PrincipalCode).HasPrincipalKey(e => e.Code);
                b.HasDiscriminator<string>("Discriminator");
            });

            modelBuilder.Entity<EncryptionDerivedDependent>();

            modelBuilder.UseEncryption(CreateProvider());
        }
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
