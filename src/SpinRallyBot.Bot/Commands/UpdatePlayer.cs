namespace SpinRallyBot.Commands;

public record UpdatePlayer(string PlayerUrl, bool ForceUpdate = false);

public class UpdatePlayerConsumer : IMediatorConsumer<UpdatePlayer> {
    private readonly IBus _bus;
    private readonly AppDbContext _db;
    private readonly ITtwClient _ttwClient;

    public UpdatePlayerConsumer(ITtwClient ttwClient, AppDbContext db, IBus bus) {
        _ttwClient = ttwClient;
        _db = db;
        _bus = bus;
        _db.ChangeTracker.StateChanged += ChangeTrackerOnStateChanged;
    }

    public async Task Consume(ConsumeContext<UpdatePlayer> context) {
        var cancellationToken = context.CancellationToken;
        var playerUrl = context.Message.PlayerUrl;
        var forceUpdate = context.Message.ForceUpdate;

        var entity = await _db.Players.FindAsync(new object[] { playerUrl }, cancellationToken);
        if (entity is null
            // || entity.Updated < DateTimeOffset.UtcNow.AddDays(-1)
            || forceUpdate) {
            var playerInfo = await _ttwClient.GetPlayerInfo(playerUrl, cancellationToken);
            if (playerInfo is null) {
                return;
            }

            entity ??= new PlayerEntity();
            entity.PlayerUrl = playerUrl;
            entity.Fio = playerInfo.Fio;
            entity.Rating = playerInfo.Rating;
            entity.Position = playerInfo.Position;
            // entity.Updated = DateTimeOffset.UtcNow;

            if (_db.Entry(entity).State is EntityState.Detached) {
                _db.Add(entity);
            }

            await _db.SaveChangesAsync(cancellationToken);
        }
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
            oldPosition));
    }
}