namespace SpinRallyBot.Queries;

public record GetCachedPlayerInfo(string PlayerUrl);

public record PlayerInfoNotFound;

public class GetCachedPlayerInfoConsumer : IMediatorConsumer<GetCachedPlayerInfo> {
    private readonly IMemoryCache _cache;
    private readonly ITtwClient _ttwClient;

    public GetCachedPlayerInfoConsumer(ITtwClient ttwClient, IMemoryCache cache) {
        _ttwClient = ttwClient;
        _cache = cache;
    }

    public async Task Consume(ConsumeContext<GetCachedPlayerInfo> context) {
        var cancellationToken = context.CancellationToken;
        var playerUrl = context.Message.PlayerUrl;

        var playerInfo = await _cache.GetOrCreateAsync($"{nameof(GetCachedPlayerInfo)}_{playerUrl}", async entry => {
            entry.SetAbsoluteExpiration(TimeSpan.FromMinutes(5));
            entry.SetSize(1);
            return await _ttwClient.GetPlayerInfo(playerUrl, cancellationToken);
            ;
        });

        if (playerInfo is not null) {
            await context.RespondAsync(playerInfo);
        } else {
            await context.RespondAsync(new PlayerInfoNotFound());
        }
    }
}