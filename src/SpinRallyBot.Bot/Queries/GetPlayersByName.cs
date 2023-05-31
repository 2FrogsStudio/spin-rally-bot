namespace SpinRallyBot.Queries;

public record GetPlayersByName(string SearchQuery);

public class GetPlayersByNameConsumer : IMediatorConsumer<GetPlayersByName> {
    private readonly ITtwClient _ttwClient;

    public GetPlayersByNameConsumer(ITtwClient ttwClient) {
        _ttwClient = ttwClient;
    }

    public async Task Consume(ConsumeContext<GetPlayersByName> context) {
        var cancellationToken = context.CancellationToken;

        var players = await _ttwClient.FindPlayers(context.Message.SearchQuery, cancellationToken);

        await context.RespondAsync(players);
    }
}