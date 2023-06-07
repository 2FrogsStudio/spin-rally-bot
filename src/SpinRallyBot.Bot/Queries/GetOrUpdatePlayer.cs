namespace SpinRallyBot.Queries;

public record GetOrUpdatePlayer(string PlayerUrl);

public record GetOrUpdatePlayerResult(
    string PlayerUrl,
    string Fio,
    float Rating,
    uint Position,
    int Subscribers,
    DateTimeOffset Updated
);

public record GetOrUpdatePlayerNotFoundResult;

public class GetOrUpdatePlayerInfoConsumer : IMediatorConsumer<GetOrUpdatePlayer> {
    private readonly IBus _bus;
    private readonly AppDbContext _db;
    private readonly ITtwClient _ttwClient;

    public GetOrUpdatePlayerInfoConsumer(ITtwClient ttwClient, AppDbContext db, IBus bus) {
        _ttwClient = ttwClient;
        _db = db;
        _bus = bus;
        _db.ChangeTracker.StateChanged += ChangeTrackerOnStateChanged;
    }

    public async Task Consume(ConsumeContext<GetOrUpdatePlayer> context) {
        var cancellationToken = context.CancellationToken;
        var playerUrl = context.Message.PlayerUrl;

        var entity = await _db.Players.FindAsync(new object[] { playerUrl }, cancellationToken);
        if (entity is null
            || entity.Updated < DateTimeOffset.UtcNow.AddDays(-1)) {
            var playerInfo = await _ttwClient.GetPlayerInfo(playerUrl, cancellationToken);
            if (playerInfo is null) {
                await context.RespondAsync(new GetOrUpdatePlayerNotFoundResult());
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

        var subscribers =
            await _db.Subscriptions.CountAsync(s => s.PlayerUrl == playerUrl, cancellationToken);

        await context.RespondAsync(new GetOrUpdatePlayerResult(
            entity.PlayerUrl,
            entity.Fio,
            entity.Rating,
            entity.Position,
            subscribers,
            TimeZoneInfo.ConvertTime(entity.Updated, Constants.RussianTimeZone)
        ));
    }

    private async void ChangeTrackerOnStateChanged(object? sender, EntityStateChangedEventArgs e) {
        if (e is not {
                Entry: {
                    Entity: PlayerEntity player,
                    OriginalValues: { } original,
                    CurrentValues: { } current
                },
                NewState: EntityState.Modified
            }) {
            return;
        }

        var oldRating = original.GetValue<float>(nameof(PlayerEntity.Rating));
        var newRating = current.GetValue<float>(nameof(PlayerEntity.Rating));

        var oldPosition = original.GetValue<uint>(nameof(PlayerEntity.Position));
        var newPosition = current.GetValue<uint>(nameof(PlayerEntity.Position));

        await _bus.Publish(new PlayerRatingChanged(
            player.PlayerUrl,
            oldRating,
            newRating,
            oldPosition,
            newPosition));
    }
}