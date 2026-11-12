using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi;
using Milvasoft.Components.OpenApi.Attributes;
using Milvasoft.Components.OpenApi.DocumentTransformers;
using Milvasoft.Components.OpenApi.OperationTransformers;
using Milvasoft.Components.OpenApi.SchemaTransformers;
using Milvasoft.Components.Swagger.Attributes;
using Milvasoft.Components.Swagger.DocumentFilters;
using Milvasoft.Components.Swagger.OperationFilters;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace Milvasoft.UnitTests.ComponentsTests.OpenApiTests;

[Trait("OpenApi Components Unit Tests", "Milvasoft.Components.OpenApi and Swagger project unit tests.")]
public class OpenApiAndSwaggerComponentTests
{
    #region Swagger

    [Fact]
    public void SwaggerFileOperationFilter_WithSelfReferencingParameterTypes_ShouldNotRecurseForever()
    {
        var operation = CreateMultipartOperation(out var schema);
        var context = new OperationFilterContext(new ApiDescription(), null, new SchemaRepository(), new OpenApiDocument(), typeof(UploadController).GetMethod(nameof(UploadController.Upload)));

        new SwaggerFileOperationFilter().Apply(operation, context);

        schema.Properties.Should().ContainKey("file");
        schema.Properties.Should().ContainKey("request");
        schema.Properties.Should().NotContainKey("date");
    }

    [Fact]
    public void SwaggerFileOperationFilter_WhenPropertyAlreadyExists_ShouldNotThrow()
    {
        var operation = CreateMultipartOperation(out var schema);
        schema.Properties = new Dictionary<string, IOpenApiSchema> { ["file"] = new OpenApiSchema { Type = JsonSchemaType.String } };
        var context = new OperationFilterContext(new ApiDescription(), null, new SchemaRepository(), new OpenApiDocument(), typeof(UploadController).GetMethod(nameof(UploadController.Upload)));

        var act = () => new SwaggerFileOperationFilter().Apply(operation, context);

        act.Should().NotThrow();
    }

    [Fact]
    public void SwaggerEndpointExcludeFilter_WithMinimalApiEndpointAndOtherMethods_ShouldOnlyRemoveExcludedOperation()
    {
        var document = new OpenApiDocument
        {
            Paths = new OpenApiPaths
            {
                ["/api/items/{id}"] = new OpenApiPathItem
                {
                    Operations = new Dictionary<HttpMethod, OpenApiOperation> { [HttpMethod.Get] = new(), [HttpMethod.Delete] = new() }
                }
            }
        };

        var excluded = CreateApiDescription("DELETE", "api/items/{id}", new SwaggerExcludeAttribute());
        var included = CreateApiDescription("GET", "api/items/{id}");

        new SwaggerEndpointExcludeFilter().Apply(document, new DocumentFilterContext([excluded, included], null, new SchemaRepository()));

        document.Paths["/api/items/{id}"].Operations.Keys.Should().BeEquivalentTo([HttpMethod.Get]);
    }

    #endregion

    #region OpenApi

    [Fact]
    public async Task EnumSchemaTransformer_ShouldKeepGeneratedValuesAndDescribeNames()
    {
        var schema = new OpenApiSchema { Type = JsonSchemaType.Integer, Enum = [1, 2] };

        await new EnumSchemaTransformer().TransformAsync(schema, CreateSchemaContext(typeof(Status)), TestContext.Current.CancellationToken);

        schema.Type.Should().Be(JsonSchemaType.Integer);
        schema.Enum.Should().HaveCount(2);
        schema.Description.Should().Contain("1 = Active").And.Contain("2 = Passive");
        schema.Extensions.Should().ContainKey("x-enum-varnames");
    }

    [Fact]
    public async Task EnumSchemaTransformer_WithULongEnum_ShouldNotOverflow()
    {
        var schema = new OpenApiSchema { Type = JsonSchemaType.Integer };

        var act = () => new EnumSchemaTransformer().TransformAsync(schema, CreateSchemaContext(typeof(BigFlags)), TestContext.Current.CancellationToken);

        await act.Should().NotThrowAsync();
        schema.Description.Should().Contain($"{ulong.MaxValue} = All");
    }

    [Fact]
    public async Task XmlCommentsTransformer_WithoutXmlDocumentationService_ShouldDoNothing()
    {
        var transformer = new XmlCommentsTransformer(new ServiceCollection().BuildServiceProvider());
        var schema = new OpenApiSchema();

        var act = () => transformer.TransformAsync(schema, CreateSchemaContext(typeof(Status)), TestContext.Current.CancellationToken);

        await act.Should().NotThrowAsync();
        schema.Description.Should().BeNull();
    }

    [Theory]
    [InlineData("api/items/{id:int}", "/api/items/{id}")]
    [InlineData("api/items/{id?}", "/api/items/{id}")]
    [InlineData("api/files/{*path}", "/api/files/{path}")]
    [InlineData("/api/items/{id:int:min(1)}?x=1", "/api/items/{id}")]
    public void OpenApiExcludeTransformer_NormalizePath_ShouldMatchOpenApiPathKeys(string relativePath, string expected)
        => OpenApiExcludeTransformer.NormalizePath(relativePath).Should().Be(expected);

    [Fact]
    public void OpenApiExcludeTransformer_IsExcluded_ShouldReadEndpointMetadata()
    {
        OpenApiExcludeTransformer.IsExcluded(CreateApiDescription("GET", "api/x", new OpenApiExcludeAttribute())).Should().BeTrue();
        OpenApiExcludeTransformer.IsExcluded(CreateApiDescription("GET", "api/x")).Should().BeFalse();
    }

    #endregion

    #region Setup

    public enum Status
    {
        Active = 1,
        Passive = 2,
    }

    [Flags]
    public enum BigFlags : ulong
    {
        None = 0,
        All = ulong.MaxValue,
    }

    public class UploadRequest
    {
        public string Name { get; set; }
        public IFormFile Attachment { get; set; }
    }

    public class UploadController
    {
        public void Upload(DateTime date, IFormFile file, UploadRequest request) { }
    }

    private static OpenApiOperation CreateMultipartOperation(out OpenApiSchema schema)
    {
        schema = new OpenApiSchema { Type = JsonSchemaType.Object };

        return new OpenApiOperation
        {
            RequestBody = new OpenApiRequestBody
            {
                Content = new Dictionary<string, IOpenApiMediaType> { ["multipart/form-data"] = new OpenApiMediaType { Schema = schema } }
            }
        };
    }

    private static ApiDescription CreateApiDescription(string httpMethod, string relativePath, params object[] metadata) => new()
    {
        HttpMethod = httpMethod,
        RelativePath = relativePath,
        ActionDescriptor = new ActionDescriptor { EndpointMetadata = [.. metadata] },
    };

    private static OpenApiSchemaTransformerContext CreateSchemaContext(Type type) => new()
    {
        DocumentName = "v1",
        JsonTypeInfo = JsonTypeInfo.CreateJsonTypeInfo(type, new JsonSerializerOptions { TypeInfoResolver = new DefaultJsonTypeInfoResolver() }),
        JsonPropertyInfo = null,
        ParameterDescription = null,
        ApplicationServices = new ServiceCollection().BuildServiceProvider(),
    };

    #endregion
}
