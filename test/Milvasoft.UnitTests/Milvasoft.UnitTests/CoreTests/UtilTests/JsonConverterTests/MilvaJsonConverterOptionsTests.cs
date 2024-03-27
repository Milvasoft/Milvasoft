using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Milvasoft.Core.Utils.JsonConverters;
using System.Text.Json;

namespace Milvasoft.UnitTests.CoreTests.UtilTests.JsonConverterTests;

public class MilvaJsonConverterOptionsTests
{
    [Fact]
    public void ConfigureCurrentMilvaJsonSerializerOptions_WithOptionsProvided_ShouldConfigureCurrentOptions()
    {
        // Arrange
        var services = new ServiceCollection();
        var options = new JsonSerializerOptions();
        var includeMilvaConverters = true;

        // Act
        var result = services.ConfigureCurrentMilvaJsonSerializerOptions(o => o.PropertyNameCaseInsensitive = true, includeMilvaConverters);

        // Assert
        result.Should().BeSameAs(MilvaJsonConverterOptions.Current);
        MilvaJsonConverterOptions.Current.PropertyNameCaseInsensitive.Should().BeTrue();
        MilvaJsonConverterOptions.Current.Converters.Should().ContainEquivalentOf(new ExceptionConverter<Exception>());
    }

    [Fact]
    public void ConfigureCurrentMilvaJsonSerializerOptions_WithOptionsNotProvided_ShouldConfigureCurrentOptions()
    {
        // Arrange
        var services = new ServiceCollection();
        var includeMilvaConverters = true;

        // Act
        var result = services.ConfigureCurrentMilvaJsonSerializerOptions(includeMilvaOptions: includeMilvaConverters);

        // Assert
        result.Should().BeSameAs(MilvaJsonConverterOptions.Current);
        MilvaJsonConverterOptions.Current.Converters.Should().ContainEquivalentOf(new ExceptionConverter<Exception>());
    }

    [Fact]
    public void ConfigureCurrentMilvaJsonSerializerOptions_WithIncludeMilvaConvertersIsFalse_ShouldNotIncludeMilvaConverters()
    {
        // Arrange
        var services = new ServiceCollection();
        var includeMilvaConverters = false;
        MilvaJsonConverterOptions.ResetCurrentOptionsToDefault();

        // Act
        var result = services.ConfigureCurrentMilvaJsonSerializerOptions(includeMilvaOptions: includeMilvaConverters);

        // Assert
        result.Should().BeSameAs(MilvaJsonConverterOptions.Current);
        MilvaJsonConverterOptions.Current.Converters.Should().NotContain(c => c.GetType() == typeof(ExceptionConverter<Exception>));
    }
}
