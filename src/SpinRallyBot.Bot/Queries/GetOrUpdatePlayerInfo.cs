namespace SpinRallyBot.Queries;

public record GetOrUpdatePlayerInfo(string PlayerUrl);

public record PlayerNotFound;

public class GetOrUpdatePlayerInfoConsumer : IMediatorConsumer<GetOrUpdatePlayerInfo> {
    private readonly AppDbContext _db;
    private readonly ITtwClient _ttwClient;

    public GetOrUpdatePlayerInfoConsumer(ITtwClient ttwClient, AppDbContext db) {
        _ttwClient = ttwClient;
        _db = db;
    }

    public async Task Consume(ConsumeContext<GetOrUpdatePlayerInfo> context) {
        var cancellationToken = context.CancellationToken;
        var playerUrl = context.Message.PlayerUrl;

        var entity = await _db.Players.FindAsync(playerUrl, cancellationToken);
        if (entity is null
            || entity.Updated < DateTimeOffset.UtcNow.AddDays(-1)) {
            var playerInfo = await _ttwClient.GetPlayerInfo(playerUrl, cancellationToken);
            if (playerInfo is null) {
                await context.RespondAsync(new PlayerNotFound());
                return;
            }

            entity ??= new PlayerEntity();
            entity.PlayerUrl = playerUrl;
            entity.Fio = playerInfo.Fio;
            entity.Rating = playerInfo.Rating;
            entity.Position = playerInfo.Position;

            if (_db.Entry(entity).State is EntityState.Detached) {
                _db.Add(entity);
            }

            await _db.SaveChangesAsync(cancellationToken);
        }

        await context.RespondAsync(new PlayerViewModel(
            entity.PlayerUrl,
            entity.Fio,
            entity.Rating,
            entity.Position,
            TimeZoneInfo.ConvertTime(entity.Updated, Constants.RussianTimeZone)
        ));
    }
}