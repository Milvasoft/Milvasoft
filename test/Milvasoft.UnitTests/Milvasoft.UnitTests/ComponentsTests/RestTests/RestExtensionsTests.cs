using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Milvasoft.Components.Rest;
using Milvasoft.Components.Rest.Enums;
using Milvasoft.Components.Rest.MilvaResponse;
using System.Text.Json;

namespace Milvasoft.UnitTests.ComponentsTests.RestTests;

[Trait("Rest Components Unit Tests", "Milvasoft.Components.Rest project unit tests.")]
public class RestExtensionsTests
{
    #region AddResponseConverters

    [Fact]
    public void AddResponseConverters_WithNullOptions_ShouldAddInterfaceConverterFactoryToJsonSerializerOptions()
    {
        // Arrange
        JsonSerializerOptions options = null;

        // Act
        var result = options.AddResponseConverters();

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void AddResponseConverters_WithValidOptions_ShouldAddInterfaceConverterFactoryToJsonSerializerOptions()
    {
        // Arrange
        var options = new JsonSerializerOptions();

        // Act
        var result = options.AddResponseConverters();

        // Assert
        result.Converters.Should().ContainSingle();
    }

    #endregion

    #region ToSuccessResponse

    [Fact]
    public void ToSuccessResponse_WithDefaultValues_ShouldReturnSuccessResponse()
    {
        // Arrange
        var data = new object();

        // Act
        var response = data.ToSuccessResponse();

        // Assert
        response.IsSuccess.Should().BeTrue();
        response.StatusCode.Should().Be(StatusCodes.Status200OK);
        response.Data.Should().Be(data);
        response.Messages.Should().HaveCount(1);
    }

    [Fact]
    public void ToSuccessResponse_WithMessage_ShouldReturnSuccessResponse()
    {
        // Arrange
        var data = new object();
        var message = "Success message";

        // Act
        var response = data.ToSuccessResponse(message);

        // Assert
        response.IsSuccess.Should().BeTrue();
        response.StatusCode.Should().Be(StatusCodes.Status200OK);
        response.Data.Should().Be(data);
        response.Messages.Should().HaveCount(1);
        response.Messages.Should().Contain(i => i.Message == message);
        response.Messages.Should().Contain(i => i.Type == MessageType.Information);
    }

    [Fact]
    public void ToSuccessResponse_WithMessageAndMessageType_ShouldReturnSuccessResponse()
    {
        // Arrange
        var data = new object();
        var message = "Success message";
        var messageType = MessageType.Warning;

        // Act
        var response = data.ToSuccessResponse(message, messageType);

        // Assert
        response.IsSuccess.Should().BeTrue();
        response.StatusCode.Should().Be(StatusCodes.Status200OK);
        response.Data.Should().Be(data);
        response.Messages.Should().HaveCount(1);
        response.Messages.Should().Contain(i => i.Message == message);
        response.Messages.Should().Contain(i => i.Type == messageType);
    }

    [Fact]
    public void ToSuccessResponse_WithResponseMessage_ShouldReturnSuccessResponse()
    {
        // Arrange
        var data = new object();
        var responseMessage = new ResponseMessage("Success message", MessageType.Information);

        // Act
        var response = data.ToSuccessResponse(responseMessage);

        // Assert
        response.IsSuccess.Should().BeTrue();
        response.StatusCode.Should().Be(StatusCodes.Status200OK);
        response.Data.Should().Be(data);
        response.Messages.Should().HaveCount(1);
        response.Messages[0].Message.Should().Be(responseMessage.Message);
        response.Messages[0].Type.Should().Be(responseMessage.Type);
    }

    #endregion

    #region ToErrorResponse

    [Fact]
    public void ToErrorResponse_WithDefaultValues_ShouldReturnErrorResponse()
    {
        // Arrange
        var data = new object();

        // Act
        var response = data.ToErrorResponse();

        // Assert
        response.IsSuccess.Should().BeFalse();
        response.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
        response.Data.Should().Be(data);
        response.Messages.Should().HaveCount(1);
    }

    [Fact]
    public void ToErrorResponse_WithMessage_ShouldReturnErrorResponse()
    {
        // Arrange
        var data = new object();
        var message = "Error message";

        // Act
        var response = data.ToErrorResponse(message);

        // Assert
        response.IsSuccess.Should().BeFalse();
        response.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
        response.Data.Should().Be(data);
        response.Messages.Should().HaveCount(1);
        response.Messages[0].Message.Should().Be(message);
        response.Messages[0].Type.Should().Be(MessageType.Warning);
    }

    [Fact]
    public void ToErrorResponse_WithMessageAndMessageType_ShouldReturnErrorResponse()
    {
        // Arrange
        var data = new object();
        var message = "Error message";
        var messageType = MessageType.Error;

        // Act
        var response = data.ToErrorResponse(message, messageType);

        // Assert
        response.IsSuccess.Should().BeFalse();
        response.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
        response.Data.Should().Be(data);
        response.Messages.Should().HaveCount(1);
        response.Messages[0].Message.Should().Be(message);
        response.Messages[0].Type.Should().Be(messageType);
    }

    [Fact]
    public void ToErrorResponse_WithResponseMessage_ShouldReturnErrorResponse()
    {
        // Arrange
        var data = new object();
        var responseMessage = new ResponseMessage("Error message", MessageType.Error);

        // Act
        var response = data.ToErrorResponse(responseMessage);

        // Assert
        response.IsSuccess.Should().BeFalse();
        response.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
        response.Data.Should().Be(data);
        response.Messages.Should().HaveCount(1);
        response.Messages[0].Message.Should().Be(responseMessage.Message);
        response.Messages[0].Type.Should().Be(responseMessage.Type);
    }

    #endregion
}
