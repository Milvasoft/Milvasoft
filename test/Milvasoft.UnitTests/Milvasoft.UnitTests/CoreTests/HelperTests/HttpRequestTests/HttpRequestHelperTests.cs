using FluentAssertions;
using Milvasoft.Core;
using Milvasoft.Core.Utils.Constants;
using System.Text;

namespace Milvasoft.UnitTests.CoreTests.HelperTests.HttpRequestTests;

[Trait("Core Unit Tests", "Milvasoft.Core project unit tests.")]
public partial class HttpRequestHelperTests
{
    #region BuildRequestMessage

    [Fact]
    public async Task BuildRequestMessage_WithDefaultValues_ShouldReturnCorrectHttpRequestMessage()
    {
        // Arrange
        var httpMethod = HttpMethod.Get;
        var url = "https://example.com";

        // Act
        using var requestMessage = httpMethod.BuildRequestMessage(url);

        // Assert
        requestMessage.Should().NotBeNull();
        requestMessage.Method.Should().Be(httpMethod);
        requestMessage.RequestUri.Should().Be(new Uri(url));
        requestMessage.Version.Should().Be(new Version(1, 1));
        requestMessage.Content.Should().BeNull();
    }

    [Fact]
    public async Task BuildRequestMessage_WithContentAndDefaultValues_ShouldUseApplicationJson()
    {
        // Arrange
        var httpMethod = HttpMethod.Post;
        var url = "https://example.com";

        // Act
        using var requestMessage = httpMethod.BuildRequestMessage(url, new { Name = "John" });

        // Assert
        requestMessage.Content.Headers.ContentType.MediaType.Should().Be(MimeTypeNames.ApplicationJson);
        requestMessage.Content.Headers.ContentType.CharSet.Should().Be(Encoding.UTF8.WebName);
        (await requestMessage.Content.ReadAsStringAsync(cancellationToken: TestContext.Current.CancellationToken)).Should().Be("{\"Name\":\"John\"}");
    }

    [Fact]
    public async Task BuildRequestMessage_WithCustomValues_ShouldReturnCorrectHttpRequestMessage()
    {
        // Arrange
        var httpMethod = HttpMethod.Post;
        var url = "https://example.com/api";
        var content = new { Name = "John", Age = 30 };
        var headers = new List<KeyValuePair<string, string>> { new("Authorization", "Bearer token") };
        var encoding = Encoding.ASCII;
        var mediaType = "application/vnd.milvasoft+json";
        var version = new Version(2, 0);

        // Act
        using var requestMessage = httpMethod.BuildRequestMessage(url, content, headers, encoding, mediaType, version);

        // Assert
        requestMessage.Should().NotBeNull();
        requestMessage.Method.Should().Be(httpMethod);
        requestMessage.RequestUri.Should().Be(new Uri(url));
        requestMessage.Version.Should().Be(version);
        requestMessage.Content.Should().NotBeNull();
        requestMessage.Content.Headers.ContentType.MediaType.Should().Be(mediaType);
        requestMessage.Content.Headers.ContentType.CharSet.Should().Be(encoding.WebName);
        (await requestMessage.Content.ReadAsStringAsync(cancellationToken: TestContext.Current.CancellationToken)).Should().Be("{\"Name\":\"John\",\"Age\":30}");
    }

    #endregion

    #region BuildRequestUrl

    [Fact]
    public void BuildRequestUrl_WithDefaultValues_ShouldReturnCorrectUrl()
    {
        // Arrange
        var protocol = "https";
        var hostName = "example.com";
        var port = "80";
        var pathName = "api";

        // Act
        var url = HttpRequestHelper.BuildRequestUrl(protocol, hostName, port, pathName);

        // Assert
        url.Should().Be("https://example.com:80/api");
    }

    [Fact]
    public void BuildRequestUrl_WithCustomValues_ShouldReturnCorrectUrl()
    {
        // Arrange
        var protocol = "http";
        var hostName = "example.com";
        var port = "8080";
        var pathName = "api/v1";
        var query = "param1=value1&param2=value2";
        var hash = "section1";

        // Act
        var url = HttpRequestHelper.BuildRequestUrl(protocol, hostName, port, pathName, query, hash);

        // Assert
        url.Should().Be("http://example.com:8080/api/v1?param1=value1&param2=value2#section1");
    }

    [Fact]
    public void BuildRequestUrl_WithEmptyQueryAndHash_ShouldReturnCorrectUrl()
    {
        // Arrange
        var protocol = "http";
        var hostName = "example.com";
        var port = "8080";
        var pathName = "api/v1";
        var query = "";
        var hash = "";

        // Act
        var url = HttpRequestHelper.BuildRequestUrl(protocol, hostName, port, pathName, query, hash);

        // Assert
        url.Should().Be("http://example.com:8080/api/v1");
    }

    [Fact]
    public void BuildRequestUrl_WithEmptyProtocol_ShouldThrowException()
    {
        // Arrange
        var protocol = "";
        var hostName = "example.com";
        var port = "8080";
        var pathName = "api/v1";

        // Act
        Action act = () => HttpRequestHelper.BuildRequestUrl(protocol, hostName, port, pathName);

        // Assert
        act.Should().Throw<Exception>().WithMessage(LocalizerKeys.InvalidUrlErrorMessage);
    }

    [Fact]
    public void BuildRequestUrl_WithInvalidUrl_ShouldThrowException()
    {
        // Arrange
        var protocol = "htxtp";
        var hostName = "example.com";
        var port = "8080";
        var pathName = "api/v1";
        var query = "param1=value1&param2=value2";
        var hash = "section1";

        // Act
        Action act = () => HttpRequestHelper.BuildRequestUrl(protocol, hostName, port, pathName, query, hash);

        // Assert
        act.Should().Throw<Exception>().WithMessage(LocalizerKeys.InvalidUrlErrorMessage);
    }

    [Theory]
    [InlineData("https://example.com/api/v1/items/~user@x;v=1?time=10:00&list=(a,b)*!")]
    [InlineData("https://[::1]:8080/api")]
    [InlineData("ftp://files.example.com/pub/file.txt")]
    [InlineData("https://xn--istanbul-xya.example.com/şehir")]
    public void BuildRequestUrlFromAddress_WithValidUrlsRejectedByOldRegex_ShouldReturnUrl(string url)
    {
        // Arrange
        var uri = new Uri(url);
        var address = uri.GetLeftPart(UriPartial.Authority);
        var pathAndQuery = url[address.Length..].TrimStart('/');

        // Act
        var result = HttpRequestHelper.BuildRequestUrlFromAddress(address, pathAndQuery);

        // Assert
        result.Should().Be(url);
    }

    [Fact]
    public void BuildRequestUrlFromAddress_WithLongHostName_ShouldCompleteQuickly()
    {
        // Arrange (nested quantifiers of the old regex backtracked catastrophically on such input)
        var address = "http://" + new string('a', 40) + "-!";
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        // Act
        Action act = () => HttpRequestHelper.BuildRequestUrlFromAddress(address, "api");

        // Assert
        act.Should().Throw<Exception>().WithMessage(LocalizerKeys.InvalidUrlErrorMessage);
        stopwatch.Elapsed.Should().BeLessThan(TimeSpan.FromSeconds(1));
    }

    #endregion

    #region BuildRequestUrlFromAddress

    [Fact]
    public void BuildRequestUrlFromAddress_WithValidAddress_ShouldReturnCorrectUrl()
    {
        // Arrange
        var address = "https://example.com";
        var pathName = "api/v1";
        var query = "param1=value1&param2=value2";
        var hash = "section1";
        var expectedUrl = "https://example.com/api/v1?param1=value1&param2=value2#section1";

        // Act
        var url = HttpRequestHelper.BuildRequestUrlFromAddress(address, pathName, query, hash);

        // Assert
        url.Should().Be(expectedUrl);
    }

    [Fact]
    public void BuildRequestUrlFromAddress_WithEmptyQueryAndHash_ShouldReturnCorrectUrl()
    {
        // Arrange
        var address = "https://example.com";
        var pathName = "api/v1";
        var query = "";
        var hash = "";
        var expectedUrl = "https://example.com/api/v1";

        // Act
        var url = HttpRequestHelper.BuildRequestUrlFromAddress(address, pathName, query, hash);

        // Assert
        url.Should().Be(expectedUrl);
    }

    [Fact]
    public void BuildRequestUrlFromAddress_WithEmptyAddress_ShouldThrowException()
    {
        // Arrange
        var address = "";
        var pathName = "api/v1";
        var query = "param1=value1&param2=value2";
        var hash = "section1";

        // Act
        Action act = () => HttpRequestHelper.BuildRequestUrlFromAddress(address, pathName, query, hash);

        // Assert
        act.Should().Throw<Exception>().WithMessage(LocalizerKeys.InvalidUrlErrorMessage);
    }

    [Fact]
    public void BuildRequestUrlFromAddress_WithInvalidUrl_ShouldThrowException()
    {
        // Arrange
        var address = "example.com";
        var pathName = "api/v1";
        var query = "param1=value1&param2=value2";
        var hash = "section1";

        // Act
        Action act = () => HttpRequestHelper.BuildRequestUrlFromAddress(address, pathName, query, hash);

        // Assert
        act.Should().Throw<Exception>().WithMessage(LocalizerKeys.InvalidUrlErrorMessage);
    }

    #endregion
}