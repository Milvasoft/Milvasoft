using FluentAssertions;
using Milvasoft.Components.Rest.Enums;
using Milvasoft.Components.Rest.MilvaResponse;
using System.Net;

namespace Milvasoft.UnitTests.ComponentsTests.RestTests.ResponseTests;

[Trait("Rest Components Unit Tests", "Milvasoft.Components.Rest project unit tests.")]
public class ResponseModelTests
{
    #region Success

    [Fact]
    public void Success_WithDefaultValues_ShouldReturnSuccessfulResponse()
    {
        // Act
        var response = Response.Success();

        // Assert
        response.IsSuccess.Should().BeTrue();
        response.StatusCode.Should().Be((int)HttpStatusCode.OK);
        response.Messages.Should().HaveCount(1);
    }

    [Fact]
    public void Success_WithMessage_ShouldReturnSuccessfulResponse()
    {
        // Arrange
        var message = "Success message";

        // Act
        var response = Response.Success(message);

        // Assert
        response.IsSuccess.Should().BeTrue();
        response.StatusCode.Should().Be((int)HttpStatusCode.OK);
        response.Messages.Should().HaveCount(1);
        response.Messages.Should().Contain(i => i.Message == message);
        response.Messages.Should().Contain(i => i.Type == MessageType.Information);
    }

    [Fact]
    public void Success_WithMessageAndMessageType_ShouldReturnSuccessfulResponse()
    {
        // Arrange
        var message = "Success message";
        var messageType = MessageType.Warning;

        // Act
        var response = Response.Success(message, messageType);

        // Assert
        response.IsSuccess.Should().BeTrue();
        response.StatusCode.Should().Be((int)HttpStatusCode.OK);
        response.Messages.Should().HaveCount(1);
        response.Messages.Should().Contain(i => i.Message == message);
        response.Messages.Should().Contain(i => i.Type == messageType);
    }

    [Fact]
    public void Success_WithResponseMessage_ShouldReturnSuccessfulResponse()
    {
        // Arrange
        var responseMessage = new ResponseMessage("Success message", MessageType.Information);

        // Act
        var response = Response.Success(responseMessage);

        // Assert
        response.IsSuccess.Should().BeTrue();
        response.StatusCode.Should().Be((int)HttpStatusCode.OK);
        response.Messages.Should().HaveCount(1);
        response.Messages[0].Message.Should().Be(responseMessage.Message);
        response.Messages[0].Type.Should().Be(responseMessage.Type);
    }

    #endregion

    #region Error

    [Fact]
    public void Error_WithDefaultValues_ShouldReturnFailedResponse()
    {
        // Act
        var response = Response.Error();

        // Assert
        response.IsSuccess.Should().BeFalse();
        response.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
        response.Messages.Should().HaveCount(1);
    }

    [Fact]
    public void Error_WithMessage_ShouldReturnFailedResponse()
    {
        // Arrange
        var message = "Error message";

        // Act
        var response = Response.Error(message);

        // Assert
        response.IsSuccess.Should().BeFalse();
        response.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
        response.Messages.Should().HaveCount(1);
        response.Messages[0].Message.Should().Be(message);
        response.Messages[0].Type.Should().Be(MessageType.Warning);
    }

    [Fact]
    public void Error_WithMessageAndMessageType_ShouldReturnFailedResponse()
    {
        // Arrange
        var message = "Error message";
        var messageType = MessageType.Error;

        // Act
        var response = Response.Error(message, messageType);

        // Assert
        response.IsSuccess.Should().BeFalse();
        response.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
        response.Messages.Should().HaveCount(1);
        response.Messages[0].Message.Should().Be(message);
        response.Messages[0].Type.Should().Be(messageType);
    }

    [Fact]
    public void Error_With_ResponseMessage_ShouldReturnFailedResponse()
    {
        // Arrange
        var responseMessage = new ResponseMessage("Error message", MessageType.Error);

        // Act
        var response = Response.Error(responseMessage);

        // Assert
        response.IsSuccess.Should().BeFalse();
        response.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
        response.Messages.Should().HaveCount(1);
        response.Messages[0].Message.Should().Be(responseMessage.Message);
        response.Messages[0].Type.Should().Be(responseMessage.Type);
    }

    [Fact]
    public void Success_ForGenericResponse_WithDefaultValues_ShouldReturnSuccessfulResponse()
    {
        // Act
        var response = Response<string>.Success(null);

        // Assert
        response.IsSuccess.Should().BeTrue();
        response.Data.Should().BeNull();
        response.StatusCode.Should().Be((int)HttpStatusCode.OK);
        response.Messages.Should().HaveCount(1);
    }

    [Fact]
    public void Success_ForGenericResponse_WithMessage_ShouldReturnSuccessfulResponse()
    {
        // Arrange
        var message = "Success message";
        var data = "This is data!";

        // Act
        var response = Response<string>.Success(data, message);

        // Assert
        response.IsSuccess.Should().BeTrue();
        response.StatusCode.Should().Be((int)HttpStatusCode.OK);
        response.Data.Should().Be(data);
        response.Messages.Should().HaveCount(1);
        response.Messages.Should().Contain(i => i.Message == message);
        response.Messages.Should().Contain(i => i.Type == MessageType.Information);
    }

    [Fact]
    public void Success_ForGenericResponse_WithMessageAndMessageType_ShouldReturnSuccessfulResponse()
    {
        // Arrange
        var message = "Success message";
        var messageType = MessageType.Warning;
        var data = "This is data!";

        // Act
        var response = Response<string>.Success(data, message, messageType);

        // Assert
        response.IsSuccess.Should().BeTrue();
        response.StatusCode.Should().Be((int)HttpStatusCode.OK);
        response.Data.Should().Be(data);
        response.Messages.Should().HaveCount(1);
        response.Messages.Should().Contain(i => i.Message == message);
        response.Messages.Should().Contain(i => i.Type == messageType);
    }

    [Fact]
    public void Success_ForGenericResponse_WithResponseMessage_ShouldReturnSuccessfulResponse()
    {
        // Arrange
        var responseMessage = new ResponseMessage("Success message", MessageType.Information);
        var data = "This is data!";

        // Act
        var response = Response<string>.Success(data, responseMessage);

        // Assert
        response.IsSuccess.Should().BeTrue();
        response.StatusCode.Should().Be((int)HttpStatusCode.OK);
        response.Data.Should().Be(data);
        response.Messages.Should().HaveCount(1);
        response.Messages[0].Message.Should().Be(responseMessage.Message);
        response.Messages[0].Type.Should().Be(responseMessage.Type);
    }

    [Fact]
    public void Error_ForGenericResponse_WithDefaultValues_ShouldReturnFailedResponse()
    {
        // Act
        var response = Response<string>.Error(null);

        // Assert
        response.IsSuccess.Should().BeFalse();
        response.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
        response.Data.Should().BeNull();
        response.Messages.Should().HaveCount(1);
    }

    [Fact]
    public void Error_ForGenericResponse_WithMessage_ShouldReturnFailedResponse()
    {
        // Arrange
        var message = "Error message";
        var data = "This is data!";

        // Act
        var response = Response<string>.Error(data, message);

        // Assert
        response.IsSuccess.Should().BeFalse();
        response.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
        response.Data.Should().Be(data);
        response.Messages.Should().HaveCount(1);
        response.Messages[0].Message.Should().Be(message);
        response.Messages[0].Type.Should().Be(MessageType.Warning);
    }

    [Fact]
    public void Error_ForGenericResponse_WithMessageAndMessageType_ShouldReturnFailedResponse()
    {
        // Arrange
        var message = "Error message";
        var messageType = MessageType.Error;
        var data = "This is data!";

        // Act
        var response = Response<string>.Error(data, message, messageType);

        // Assert
        response.IsSuccess.Should().BeFalse();
        response.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
        response.Data.Should().Be(data);
        response.Messages.Should().HaveCount(1);
        response.Messages[0].Message.Should().Be(message);
        response.Messages[0].Type.Should().Be(messageType);
    }

    [Fact]
    public void Error_ForGenericResponse_With_ResponseMessage_ShouldReturnFailedResponse()
    {
        // Arrange
        var responseMessage = new ResponseMessage("Error message", MessageType.Error);
        var data = "This is data!";

        // Act
        var response = Response<string>.Error(data, responseMessage);

        // Assert
        response.IsSuccess.Should().BeFalse();
        response.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
        response.Data.Should().Be(data);
        response.Messages.Should().HaveCount(1);
        response.Messages[0].Message.Should().Be(responseMessage.Message);
        response.Messages[0].Type.Should().Be(responseMessage.Type);
    }

    #endregion
}
