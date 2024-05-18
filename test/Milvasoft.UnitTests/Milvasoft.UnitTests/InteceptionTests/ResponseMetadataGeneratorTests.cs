using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Milvasoft.Attributes.Annotations;
using Milvasoft.Components.Rest.MilvaResponse;
using Milvasoft.Core.Abstractions;
using Milvasoft.Core.Abstractions.Localization;
using Milvasoft.Core.Utils.Constants;
using Milvasoft.Interception.Builder;
using Milvasoft.Interception.Decorator;
using Milvasoft.Interception.Interceptors.Response;
using Milvasoft.Types.Classes;
using System.ComponentModel;

namespace Milvasoft.UnitTests.InteceptionTests;

[Trait("Interceptors Unit Tests", "Unit tests for Milvasoft.Interception project interceptors.")]
public class ResponseMetadataGeneratorTests
{
    [Fact]
    public void MethodReturnTypeIsValueResponseTyped_WithMetadataGenerator_ShouldModifyResponseCorrecly()
    {
        // Arrange
        var services = GetServices();
        var options = services.GetService<IResponseInterceptionOptions>();
        var generator = new ResponseMetadataGenerator(options, services);
        var sut = services.GetService<ISomeInterface>();

        // Act
        var returnValue = sut.MethodReturnTypeIsValueResponseTyped();
        generator.GenerateMetadata(returnValue);

        // Assert
        returnValue.Messages[0].Message.Should().Be($"localized_{LocalizerKeys.Successful}");
        returnValue.Data.Should().Be(1);
        returnValue.Metadatas.Should().NotBeEmpty();
        returnValue.Metadatas[0].Type.Should().Be("Int32");
        returnValue.Metadatas[0].LocalizedName.Should().Be("Data");
        returnValue.Metadatas[0].Display.Should().BeTrue();
    }

    [Fact]
    public void MethodReturnTypeIsCollectionResponseTyped_WithMetadataGenerator_ShouldModifyResponseCorrecly()
    {
        // Arrange
        var services = GetServices();
        var options = services.GetService<IResponseInterceptionOptions>();
        var generator = new ResponseMetadataGenerator(options, services);
        var sut = services.GetService<ISomeInterface>();

        // Act
        var returnValue = sut.MethodReturnTypeIsCollectionResponseTyped();
        generator.GenerateMetadata(returnValue);

        // Assert
        returnValue.Messages[0].Message.Should().Be($"localized_{LocalizerKeys.Successful}");
        returnValue.Data[0].Should().Be(1);
        returnValue.Metadatas.Should().NotBeEmpty();
        returnValue.Metadatas[0].Type.Should().Be("List.Int32");
        returnValue.Metadatas[0].LocalizedName.Should().Be("Data");
        returnValue.Metadatas[0].Display.Should().BeTrue();
    }

    [Fact]
    public void MethodReturnTypeIsComplexResponseTyped_WithMetadataGenerator_ShouldModifyResponseCorrecly()
    {
        // Arrange
        var services = GetServices();
        var options = services.GetService<IResponseInterceptionOptions>();
        var generator = new ResponseMetadataGenerator(options, services);
        var sut = services.GetService<ISomeInterface>();

        // Act
        var returnValue = sut.MethodReturnTypeIsComplexResponseTyped();
        generator.GenerateMetadata(returnValue);

        // Assert
        returnValue.Messages[0].Message.Should().Be($"localized_{LocalizerKeys.Successful}");
        returnValue.Data.Should().BeOfType<SomeComplexClass>();
        returnValue.Metadatas.Should().NotBeEmpty();
        returnValue.Metadatas.Find(m => m.Name == "IntProp").LocalizedName.Should().Be($"localized_SomeComplexClass.IntProp");
        returnValue.Metadatas.Find(m => m.Name == "IntProp").Pinned.Should().BeTrue();
        returnValue.Metadatas.Find(m => m.Name == "StringProp").Mask.Should().BeTrue();
        returnValue.Metadatas.Find(m => m.Name == "StringProp").DefaultValue.Should().Be("localized_-");
        returnValue.Data.StringProp.Should().Contain("*");
        returnValue.Metadatas.Find(m => m.Name == "BoolProp").Should().BeNull();
        returnValue.Data.BoolProp.Should().BeFalse();
        returnValue.Metadatas.Find(m => m.Name == "DecimalProp").DecimalPrecision.Precision.Should().Be(18);
        returnValue.Metadatas.Find(m => m.Name == "DecimalProp").DecimalPrecision.Scale.Should().Be(2);
        returnValue.Metadatas.Find(m => m.Name == "DecimalProp").DisplayFormat.Should().Be("{DecimalProp}₺");
        returnValue.Metadatas.Find(m => m.Name == "ListProp").FilterFormat.Should().Be("ListProp[SomeProp]");
        returnValue.Metadatas.Find(m => m.Name == "ComplexClass").Display.Should().BeFalse();
        returnValue.Metadatas.Find(m => m.Name == "ComplexClass").Metadatas.Should().NotBeEmpty();
        returnValue.Metadatas.Find(m => m.Name == "ComplexClass").Metadatas.Find(m => m.Name == "DateProp").TooltipFormat.Should().Be("dddd, dd MMMM yyyy");
        returnValue.Metadatas.Find(m => m.Name == "ComplexClass").Metadatas.Find(m => m.Name == "EnumProp").Display.Should().BeTrue();
    }

    #region Setup

    public interface ISomeInterface : IInterceptable
    {
        Response<int> MethodReturnTypeIsValueResponseTyped();
        Response<List<int>> MethodReturnTypeIsCollectionResponseTyped();
        Response<SomeComplexClass> MethodReturnTypeIsComplexResponseTyped();
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Minor Code Smell", "S2344:Enumeration type names should not have \"Flags\" or \"Enum\" suffixes", Justification = "<Pending>")]
    public enum SomeEnum
    {
        None,
    }

    [Translate]
    public class SomeComplexClass
    {
        [Pinned]
        [HideByRole("Admin")]
        public int IntProp { get; set; }

        [MaskByRole("NotAllowed")]
        [DefaultValue("-")]
        public string StringProp { get; set; }

        [HideByRole("Hide")]
        public bool BoolProp { get; set; } = true;

        [DecimalPrecision(18, 2)]
        [DisplayFormat("{DecimalProp}₺")]
        public decimal DecimalProp { get; set; }

        [Filterable(false, FilterFormat = "ListProp[SomeProp]")]
        public List<int> ListProp { get; set; }

        [Browsable(false)]
        public AnotherComplexClass ComplexClass { get; set; }
    }

    public class AnotherComplexClass
    {
        [TooltipFormat("dddd, dd MMMM yyyy")]
        public DateTime DateProp { get; set; }

        [HideByRole("NotHide")]
        public SomeEnum EnumProp { get; set; }
    }

    public class SomeClass : ISomeInterface
    {
        public virtual Response<int> MethodReturnTypeIsValueResponseTyped() => Response<int>.Success(1);

        public virtual Response<List<int>> MethodReturnTypeIsCollectionResponseTyped() => Response<List<int>>.Success([1]);

        public virtual Response<SomeComplexClass> MethodReturnTypeIsComplexResponseTyped()
        {
            var data = new SomeComplexClass
            {
                IntProp = 1,
                StringProp = "string",
                DecimalProp = 987.78899889M,
                BoolProp = true,
                ComplexClass = new AnotherComplexClass
                {
                    DateProp = DateTime.Now,
                    EnumProp = SomeEnum.None
                },
                ListProp = []
            };

            return Response<SomeComplexClass>.Success(data);
        }
    }

    public class TestLocalizer : IMilvaLocalizer
    {
        public LocalizedValue this[string key] => new(key, $"localized_{key}");

        public LocalizedValue this[string key, params object[] arguments] => throw new NotImplementedException();

        public IEnumerable<LocalizedValue> GetAllStrings(bool includeParentCultures) => throw new NotImplementedException();
        public LocalizedValue GetWithCulture(string key, string culture) => throw new NotImplementedException();
        public LocalizedValue GetWithCulture(string key, string culture, params object[] arguments) => throw new NotImplementedException();
    }

    private static bool HideByRoleFunc(IServiceProvider serviceProvider, HideByRoleAttribute hideByRoleAttribute)
        => hideByRoleAttribute.Roles.Contains("Hide");
    private static bool MaskByRoleFunc(IServiceProvider serviceProvider, MaskByRoleAttribute maskByRoleAttribute)
        => maskByRoleAttribute.Roles.Contains("NotAllowed");

    private static ServiceProvider GetServices()
    {
        var builder = new InterceptionBuilder(new ServiceCollection());

        builder.Services.AddScoped<ISomeInterface, SomeClass>();
        builder.Services.AddScoped<IMilvaLocalizer, TestLocalizer>();

        builder.Services.AddMilvaInterception([typeof(ISomeInterface)])
                        .WithResponseInterceptor(opt =>
                        {
                            opt.HideByRoleFunc = HideByRoleFunc;
                            opt.MaskByRoleFunc = MaskByRoleFunc;
                        });

        var serviceProvider = builder.Services.BuildServiceProvider();

        return serviceProvider;
    }

    #endregion
}
