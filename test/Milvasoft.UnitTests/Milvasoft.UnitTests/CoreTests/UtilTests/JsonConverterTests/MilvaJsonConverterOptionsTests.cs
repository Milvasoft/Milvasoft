using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Milvasoft.Core.Utils.JsonConverters;

namespace Milvasoft.UnitTests.CoreTests.UtilTests.JsonConverterTests;

[System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "xUnit1004:Test methods should not be skipped", Justification = "<Pending>")]
public class MilvaJsonConverterOptionsTests
{
    [Fact(Skip = "Should be run separately when necessary because of static object access.")]
    public void ConfigureCurrentMilvaJsonSerializerOptions_WithOptionsProvided_ShouldConfigureCurrentOptions()
    {
        // Arrange
        var services = new ServiceCollection();
        var includeMilvaConverters = true;

        // Act
        var result = services.ConfigureCurrentMilvaJsonSerializerOptions(o => o.PropertyNameCaseInsensitive = true, includeMilvaConverters);

        // Assert
        result.Should().BeSameAs(MilvaJsonConverterOptions.Current);
        MilvaJsonConverterOptions.Current.PropertyNameCaseInsensitive.Should().BeTrue();
        MilvaJsonConverterOptions.Current.Converters.Should().ContainEquivalentOf(new ExceptionConverter<Exception>());
    }

    [Fact(Skip = "Should be run separately when necessary because of static object access.")]
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

    [Fact(Skip = "Should be run separately when necessary because of static object access.")]
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
