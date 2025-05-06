using System.Net;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;

namespace SpinRallyBot;

public class TtwClient(HttpClient httpClient, IHtmlParser htmlParser) : ITtwClient {
    public async Task<PlayerInfo?> GetPlayerInfo(string playerUrl, CancellationToken cancellationToken) {
        var request = new HttpRequestMessage(HttpMethod.Get, playerUrl);
        HttpResponseMessage response = await httpClient.SendAsync(request, cancellationToken);
        if (response.StatusCode is HttpStatusCode.NotFound) {
            return null;
        }

        response.EnsureSuccessStatusCode();

        IHtmlDocument doc =
            await htmlParser.ParseDocumentAsync(await response.Content.ReadAsStreamAsync(cancellationToken));

        string fio = doc.QuerySelector("div.player-page h1 span")?.Text() ?? throw new InvalidOperationException();

        IElement? playerPage = doc.QuerySelector("div.player-page");

        float rating = float.Parse(playerPage?.QuerySelector("div.player-all-games th.rating-rating-cell")?.Text() ??
                                   throw new InvalidOperationException());
        uint position = uint.Parse(playerPage.QuerySelector("div.header-position")?.Text() ??
                                   throw new InvalidOperationException());

        return new PlayerInfo(
            playerUrl,
            fio,
            rating,
            position);
    }

    public async Task<Player[]> FindPlayers(string searchQuery, CancellationToken cancellationToken) {
        if (string.IsNullOrWhiteSpace(searchQuery)) {
            return [];
        }

        var request = new HttpRequestMessage(HttpMethod.Post, "/wp-admin/admin-ajax.php") {
            Content = new FormUrlEncodedContent([
                new KeyValuePair<string, string>("action", "get_players_by_name"),
                new KeyValuePair<string, string>("name", searchQuery)
            ])
        };

        HttpResponseMessage response = await httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        IHtmlDocument doc =
            await htmlParser.ParseDocumentAsync(await response.Content.ReadAsStreamAsync(cancellationToken));

        IHtmlCollection<IElement> htmlPlayers = doc.QuerySelectorAll("div a");
        return htmlPlayers.Select(h => new Player(
            h.Attributes.GetNamedItem("href")?.Text() ?? throw new InvalidOperationException(),
            h.Text()
        )).ToArray();
    }
}
