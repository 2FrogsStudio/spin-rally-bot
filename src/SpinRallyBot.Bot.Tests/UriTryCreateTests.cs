namespace SpinRallyBot.Bot.Tests;

public class UriTryCreateTests {
    [Theory]
    [InlineData("https://r.ttw.ru/players/?id=52a31ad")]
    [InlineData("/players/?id=52a31ad")]
    public void TestUriTryCreate(string uriString) {
        var baseUri = new Uri(Constants.RttwUrl);
        if (!Uri.TryCreate(uriString, UriKind.Absolute, out Uri? uri)
            || string.IsNullOrWhiteSpace(uri.Host)) {
            uri = new Uri(baseUri, uriString);
        }

        uri.IsAbsoluteUri.Should().BeTrue();
        uri.AbsolutePath.Should().Be("/players/");
        uri.PathAndQuery.Should().Be("/players/?id=52a31ad");
    }
}
