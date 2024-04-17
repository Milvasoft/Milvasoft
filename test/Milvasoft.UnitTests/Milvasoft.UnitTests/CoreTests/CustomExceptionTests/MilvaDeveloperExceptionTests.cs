using FluentAssertions;
using Milvasoft.Core.Exceptions;

namespace Milvasoft.UnitTests.CoreTests.CustomExceptionTests;

[Trait("Core Unit Tests", "Milvasoft.Core project unit tests.")]
public class MilvaDeveloperExceptionTests
{
    [Fact]
    public void Constructor_WithNoParameters_ShouldInitializeCorrectly()
    {
        // Arrange & Act
        var ex = new MilvaDeveloperException();

        // Assert
        ex.Message.Should().Be($"{MilvaException.Base}Exception");
        ex.ExceptionCode.Should().Be((int)MilvaException.Base);
    }

    [Fact]
    public void Constructor_WithParametersWithMessageOrLocalizerKey_ShouldInitializeCorrectly()
    {
        // Arrange & Act
        var message = "some message";
        var ex = new MilvaDeveloperException(message);

        // Assert
        ex.Message.Should().Be(message);
        ex.ExceptionCode.Should().Be((int)MilvaException.Base);
    }

    [Fact]
    public void Constructor_WithParametersWithMessageOrLocalizerKeyAndInnerException_ShouldInitializeCorrectly()
    {
        // Arrange & Act
        var message = "some message";
        var innerExMessage = "some inner message";
        var innerEx = new MilvaDeveloperException(innerExMessage);
        var ex = new MilvaDeveloperException(message, innerEx);

        // Assert
        ex.Message.Should().Be(message);
        ex.ExceptionCode.Should().Be((int)MilvaException.Base);
        ex.InnerException.Should().Be(innerEx);
    }
}
