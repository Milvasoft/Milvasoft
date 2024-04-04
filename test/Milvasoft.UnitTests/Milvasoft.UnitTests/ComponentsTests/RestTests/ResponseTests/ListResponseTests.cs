using FluentAssertions;
using Milvasoft.Components.Rest.Enums;
using Milvasoft.Components.Rest.Response;
using System.Net;

namespace Milvasoft.UnitTests.ComponentsTests.RestTests.ResponseTests;

public class ListResponseTests
{
    #region Success 

    [Fact]
    public void Success_WithDefaultValues_ShouldReturnSuccessfulResponse()
    {
        // Act
        var response = ListResponse<int>.Success();

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
        List<int> data = [1];

        // Act
        var response = ListResponse<int>.Success(data, message);

        // Assert
        response.IsSuccess.Should().BeTrue();
        response.StatusCode.Should().Be((int)HttpStatusCode.OK);
        response.Data.Should().BeEquivalentTo(data);
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
        List<int> data = [1];

        // Act
        var response = ListResponse<int>.Success(data, message, messageType);

        // Assert
        response.IsSuccess.Should().BeTrue();
        response.StatusCode.Should().Be((int)HttpStatusCode.OK);
        response.Data.Should().BeEquivalentTo(data);
        response.Messages.Should().HaveCount(1);
        response.Messages.Should().Contain(i => i.Message == message);
        response.Messages.Should().Contain(i => i.Type == messageType);
    }

    [Fact]
    public void Success_WithResponseMessage_ShouldReturnSuccessfulResponse()
    {
        // Arrange
        var responseMessage = new ResponseMessage("Success message", MessageType.Information);
        List<int> data = [1];

        // Act
        var response = ListResponse<int>.Success(data, responseMessage);

        // Assert
        response.IsSuccess.Should().BeTrue();
        response.StatusCode.Should().Be((int)HttpStatusCode.OK);
        response.Data.Should().BeEquivalentTo(data);
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
        var response = ListResponse<int>.Error();

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
        List<int> data = [1];

        // Act
        var response = ListResponse<int>.Error(data, message);

        // Assert
        response.IsSuccess.Should().BeFalse();
        response.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
        response.Data.Should().BeEquivalentTo(data);
        response.Messages.Should().HaveCount(1);
        response.Messages[0].Message.Should().Be(message);
        response.Messages[0].Type.Should().Be(MessageType.Information);
    }

    [Fact]
    public void Error_WithMessageAndMessageType_ShouldReturnFailedResponse()
    {
        // Arrange
        var message = "Error message";
        var messageType = MessageType.Error;
        List<int> data = [1];

        // Act
        var response = ListResponse<int>.Error(data, message, messageType);

        // Assert
        response.IsSuccess.Should().BeFalse();
        response.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
        response.Data.Should().BeEquivalentTo(data);
        response.Messages.Should().HaveCount(1);
        response.Messages[0].Message.Should().Be(message);
        response.Messages[0].Type.Should().Be(messageType);
    }

    [Fact]
    public void Error_WithResponseMessage_ShouldReturnFailedResponse()
    {
        // Arrange
        var responseMessage = new ResponseMessage("Error message", MessageType.Error);
        List<int> data = [1];

        // Act
        var response = ListResponse<int>.Error(data, responseMessage);

        // Assert
        response.IsSuccess.Should().BeFalse();
        response.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
        response.Data.Should().BeEquivalentTo(data);
        response.Messages.Should().HaveCount(1);
        response.Messages[0].Message.Should().Be(responseMessage.Message);
        response.Messages[0].Type.Should().Be(responseMessage.Type);
    }

    #endregion
}
