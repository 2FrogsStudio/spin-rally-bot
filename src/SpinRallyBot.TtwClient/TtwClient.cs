using System.Net;
using AngleSharp.Dom;
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
        var response = await _httpClient.SendAsync(request, cancellationToken);
        if (response.StatusCode is HttpStatusCode.NotFound) {
            return null;
        }

        response.EnsureSuccessStatusCode();

        var doc = await _htmlParser.ParseDocumentAsync(await response.Content.ReadAsStreamAsync(cancellationToken));

        var fio = doc.QuerySelector("div.player-page h1 span")!.Text();

        var playerPage = doc.QuerySelector("div.player-page");

        var rating = float.Parse(playerPage!.QuerySelector("div.player-all-games th.rating-rating-cell")!.Text());
        var position = uint.Parse(playerPage.QuerySelector("div.header-position")!.Text());

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

        var response = await _httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        var doc = await _htmlParser.ParseDocumentAsync(await response.Content.ReadAsStreamAsync(cancellationToken));

        var htmlPlayers = doc.QuerySelectorAll("div a");
        return htmlPlayers.Select(h => new Player(
            h.Attributes.GetNamedItem("href")!.Text(),
            h.Text()
        )).ToArray();
    }
}