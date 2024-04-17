using FluentAssertions;
using Milvasoft.Core.Exceptions;

namespace Milvasoft.UnitTests.CoreTests.CustomExceptionTests;

[Trait("Core Unit Tests", "Milvasoft.Core project unit tests.")]
public class MilvaUserFriendlyExceptionTests
{
    [Fact]
    public void Constructor_WithNoParameters_ShouldInitializeCorrectly()
    {
        // Arrange & Act
        var ex = new MilvaUserFriendlyException();

        // Assert
        ex.Message.Should().Be($"{MilvaException.Base}Exception");
        ex.ExceptionCode.Should().Be((int)MilvaException.Base);
    }

    [Fact]
    public void Constructor_WithParametersWithMessageOrLocalizerKey_ShouldInitializeCorrectly()
    {
        // Arrange & Act
        var message = "some message";
        var ex = new MilvaUserFriendlyException(message);

        // Assert
        ex.Message.Should().Be(message);
        ex.ExceptionCode.Should().Be((int)MilvaException.Base);
    }

    [Fact]
    public void Constructor_WithParametersWithMessageOrLocalizerKeyAndUseLocalizerKey_ShouldInitializeCorrectly()
    {
        // Arrange & Act
        var message = "some message";
        var ex = new MilvaUserFriendlyException(message, false);

        // Assert
        ex.Message.Should().Be(message);
        ex.ExceptionCode.Should().Be((int)MilvaException.Base);
        ex.UseLocalizerKey.Should().BeFalse();
    }

    [Fact]
    public void Constructor_WithParametersWithMessageOrLocalizerKeyAndExceptionCode_ShouldInitializeCorrectly()
    {
        // Arrange & Act
        var message = "some message";
        var ex = new MilvaUserFriendlyException(message, MilvaException.AnotherLoginExists);

        // Assert
        ex.Message.Should().Be(message);
        ex.ExceptionCode.Should().Be((int)MilvaException.AnotherLoginExists);
        ex.UseLocalizerKey.Should().BeTrue();
    }

    [Fact]
    public void Constructor_WithParametersWithMessageOrLocalizerKeyAndExceptionCodeAndExceptionObjects_ShouldInitializeCorrectly()
    {
        // Arrange & Act
        var message = "some message";
        var exObject = 2.5M;
        var ex = new MilvaUserFriendlyException(message, MilvaException.AnotherLoginExists, exObject);

        // Assert
        ex.Message.Should().Be(message);
        ex.ExceptionCode.Should().Be((int)MilvaException.AnotherLoginExists);
        ex.UseLocalizerKey.Should().BeTrue();
        ex.ExceptionObject[0].Should().Be(exObject);
    }

    [Fact]
    public void Constructor_WithParametersWithMessageOrLocalizerKeyAndExceptionCodeAsIntAndExceptionObjects_ShouldInitializeCorrectly()
    {
        // Arrange & Act
        var message = "some message";
        var exObject = 2.5M;
        var ex = new MilvaUserFriendlyException(message, 18, exObject);

        // Assert
        ex.Message.Should().Be(message);
        ex.ExceptionCode.Should().Be((int)MilvaException.AnotherLoginExists);
        ex.UseLocalizerKey.Should().BeTrue();
        ex.ExceptionObject[0].Should().Be(exObject);
    }

    [Fact]
    public void Constructor_WithParametersWithMessageOrLocalizerKeyAndExceptionCodeAsIntAndUseLocalizerKey_ShouldInitializeCorrectly()
    {
        // Arrange & Act
        var message = "some message";
        var ex = new MilvaUserFriendlyException(message, 18, true);

        // Assert
        ex.Message.Should().Be(message);
        ex.ExceptionCode.Should().Be((int)MilvaException.AnotherLoginExists);
        ex.UseLocalizerKey.Should().BeTrue();
    }

    [Fact]
    public void Constructor_WithParametersWithMessageOrLocalizerKeyAndExceptionCodeAsInt_ShouldInitializeCorrectly()
    {
        // Arrange & Act
        var message = "some message";
        var ex = new MilvaUserFriendlyException(message, 18);

        // Assert
        ex.Message.Should().Be(message);
        ex.ExceptionCode.Should().Be((int)MilvaException.AnotherLoginExists);
        ex.UseLocalizerKey.Should().BeTrue();
    }

    [Fact]
    public void Constructor_WithParametersWithExceptionCode_ShouldInitializeCorrectly()
    {
        // Arrange & Act
        var ex = new MilvaUserFriendlyException(MilvaException.AnotherLoginExists);

        // Assert
        ex.Message.Should().Be($"{MilvaException.AnotherLoginExists}Exception");
        ex.ExceptionCode.Should().Be((int)MilvaException.AnotherLoginExists);
        ex.UseLocalizerKey.Should().BeTrue();
    }

    [Fact]
    public void Constructor_WithParametersWithExceptionCodeAndExceptionObjects_ShouldInitializeCorrectly()
    {
        // Arrange & Act
        var exObject = 2.5M;
        var ex = new MilvaUserFriendlyException(MilvaException.AnotherLoginExists, exObject);

        // Assert
        ex.Message.Should().Be($"{MilvaException.AnotherLoginExists}Exception");
        ex.ExceptionCode.Should().Be((int)MilvaException.AnotherLoginExists);
        ex.UseLocalizerKey.Should().BeTrue();
        ex.ExceptionObject[0].Should().Be(exObject);
    }

    [Fact]
    public void Constructor_WithParametersWithMessageOrLocalizerKeyAndExceptionObjects_ShouldInitializeCorrectly()
    {
        // Arrange & Act
        var message = "some message";
        var exObject = 2.5M;
        var ex = new MilvaUserFriendlyException(message, exObject);

        // Assert
        ex.Message.Should().Be(message);
        ex.ExceptionCode.Should().Be((int)MilvaException.Base);
        ex.UseLocalizerKey.Should().BeTrue();
        ex.ExceptionObject[0].Should().Be(exObject);
    }

    [Fact]
    public void Constructor_WithParametersWithMessageOrLocalizerKeyAndUseLocalizerKeyAndExceptionObjects_ShouldInitializeCorrectly()
    {
        // Arrange & Act
        var message = "some message";
        var exObject = 2.5M;
        var ex = new MilvaUserFriendlyException(message, false, exObject);

        // Assert
        ex.Message.Should().Be(message);
        ex.ExceptionCode.Should().Be((int)MilvaException.Base);
        ex.UseLocalizerKey.Should().BeFalse();
        ex.ExceptionObject[0].Should().Be(exObject);
    }

    [Fact]
    public void Constructor_WithParametersWithMessageOrLocalizerKeyAndInnerException_ShouldInitializeCorrectly()
    {
        // Arrange & Act
        var message = "some message";
        var innerExMessage = "some inner message";
        var innerEx = new MilvaUserFriendlyException(innerExMessage);
        var ex = new MilvaUserFriendlyException(message, innerEx);

        // Assert
        ex.Message.Should().Be(message);
        ex.ExceptionCode.Should().Be((int)MilvaException.Base);
        ex.InnerException.Should().Be(innerEx);
    }

    [Fact]
    public void Constructor_WithParametersWithMessageOrLocalizerKeyAndExceptionCodeAsIntAndInnerException_ShouldInitializeCorrectly()
    {
        // Arrange & Act
        var message = "some message";
        var innerExMessage = "some inner message";
        var innerEx = new MilvaUserFriendlyException(innerExMessage);
        var ex = new MilvaUserFriendlyException(message, 18, innerEx);

        // Assert
        ex.Message.Should().Be(message);
        ex.ExceptionCode.Should().Be((int)MilvaException.AnotherLoginExists);
        ex.InnerException.Should().Be(innerEx);
    }
}
