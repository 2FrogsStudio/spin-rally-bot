using System.Net;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;

namespace SpinRallyBot;

public class TtwClient : ITtwClient {
    private readonly IHtmlParser _htmlParser;
    private readonly HttpClient _httpClient;

    public TtwClient(HttpClient httpClient, IHtmlParser htmlParser) {
        _httpClient = httpClient;
        _htmlParser = htmlParser;
    }

    public async Task<PlayerInfo?> GetPlayerInfo(string playerUrl, CancellationToken cancellationToken) {
        var request = new HttpRequestMessage(HttpMethod.Get, playerUrl);
        HttpResponseMessage response = await _httpClient.SendAsync(request, cancellationToken);
        if (response.StatusCode is HttpStatusCode.NotFound) {
            return null;
        }

        response.EnsureSuccessStatusCode();

        IHtmlDocument doc =
            await _htmlParser.ParseDocumentAsync(await response.Content.ReadAsStreamAsync(cancellationToken));

        string fio = doc.QuerySelector("div.player-page h1 span")!.Text();

        IElement? playerPage = doc.QuerySelector("div.player-page");

        float rating = float.Parse(playerPage!.QuerySelector("div.player-all-games th.rating-rating-cell")!.Text());
        uint position = uint.Parse(playerPage.QuerySelector("div.header-position")!.Text());

        return new PlayerInfo(
            playerUrl,
            fio,
            rating,
            position);
    }

    public async Task<Player[]> FindPlayers(string searchQuery, CancellationToken cancellationToken) {
        if (string.IsNullOrWhiteSpace(searchQuery)) {
            return Array.Empty<Player>();
        }

        var request = new HttpRequestMessage(HttpMethod.Post, "/wp-admin/admin-ajax.php") {
            Content = new FormUrlEncodedContent(new KeyValuePair<string, string>[] {
                new("action", "get_players_by_name"),
                new("name", searchQuery)
            })
        };

        HttpResponseMessage response = await _httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        IHtmlDocument doc =
            await _htmlParser.ParseDocumentAsync(await response.Content.ReadAsStreamAsync(cancellationToken));

        IHtmlCollection<IElement> htmlPlayers = doc.QuerySelectorAll("div a");
        return htmlPlayers.Select(h => new Player(
            h.Attributes.GetNamedItem("href")!.Text(),
            h.Text()
        )).ToArray();
    }
}
