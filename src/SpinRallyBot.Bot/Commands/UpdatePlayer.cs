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
    }

    public async Task Consume(ConsumeContext<UpdatePlayer> context) {
        CancellationToken cancellationToken = context.CancellationToken;
        string playerUrl = context.Message.PlayerUrl;
        bool forceUpdate = context.Message.ForceUpdate;

        PlayerEntity? entity = await _db.Players.FindAsync(new object[] { playerUrl }, cancellationToken);
        if (entity is null || forceUpdate) {
            PlayerInfo? playerInfo = await _ttwClient.GetPlayerInfo(playerUrl, cancellationToken);
            if (playerInfo is null) {
                return;
            }

            float? oldRating = null;
            uint? oldPosition = null;
            bool isRatingChanged = false;

            if (entity is not null) {
                oldRating = entity.Rating;
                oldPosition = entity.Position;
                isRatingChanged = Math.Round(Math.Abs(playerInfo.Rating - oldRating.Value), 2) > 0.01;
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

            if (isRatingChanged) {
                await _bus.Publish(
                    new PlayerRatingChanged(
                        playerInfo.PlayerUrl,
                        oldRating.GetValueOrDefault(),
                        oldPosition.GetValueOrDefault()
                    ), cancellationToken);
            }
        }
    }
}
