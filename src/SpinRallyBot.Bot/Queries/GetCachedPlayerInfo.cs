using SpinRallyBot.Commands;

namespace SpinRallyBot.Queries;

public record GetCachedPlayerInfo(string PlayerUrl);

public record PlayerInfoNotFound;

public class GetCachedPlayerInfoConsumer : IMediatorConsumer<GetCachedPlayerInfo> {
    private readonly IMemoryCache _cache;
    private readonly ITtwClient _ttwClient;
    private readonly IScopedMediator _mediator;

    public GetCachedPlayerInfoConsumer(ITtwClient ttwClient, IMemoryCache cache, IScopedMediator mediator) {
        _ttwClient = ttwClient;
        _cache = cache;
        _mediator = mediator;
    }

    public async Task Consume(ConsumeContext<GetCachedPlayerInfo> context) {
        var cancellationToken = context.CancellationToken;
        var playerUrl = context.Message.PlayerUrl;
        
        var playerInfo = await _cache.GetOrCreateAsync($"{nameof(GetCachedPlayerInfo)}_{playerUrl}", async entry => {
            entry.SetAbsoluteExpiration(TimeSpan.FromHours(1));
            entry.SetSize(1);
            return await _ttwClient.GetPlayerInfo(playerUrl, cancellationToken);
        });

        if (playerInfo is not null) {
            await _mediator.Send(new AddPlayerToDb(playerInfo), cancellationToken);
            await context.RespondAsync(playerInfo);
        } else {
            await context.RespondAsync(new PlayerInfoNotFound());
        }
    }
}