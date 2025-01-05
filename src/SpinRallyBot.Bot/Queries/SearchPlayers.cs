namespace SpinRallyBot.Queries;

public record SearchPlayers(string SearchQuery);

public record SearchPlayersResult((string Fio, string PlayerUrl)[] Players);

public class SearchPlayersConsumer : IMediatorConsumer<SearchPlayers> {
    private readonly ITtwClient _ttwClient;

    public SearchPlayersConsumer(ITtwClient ttwClient) {
        _ttwClient = ttwClient;
    }

    public async Task Consume(ConsumeContext<SearchPlayers> context) {
        CancellationToken cancellationToken = context.CancellationToken;

        Player[] players = await _ttwClient.FindPlayers(context.Message.SearchQuery, cancellationToken);

        await context.RespondAsync(new SearchPlayersResult(
            players
                .OrderBy(p => p.Fio)
                .Select(p => (p.Fio, p.PlayerUrl))
                .ToArray()));
    }
}
