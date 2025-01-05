namespace SpinRallyBot.Queries;

public record GetPlayer(string PlayerUrl);

public record GetPlayerNotFoundResult;

public record GetPlayerResult(
    string PlayerUrl,
    string Fio,
    float Rating,
    uint Position,
    int Subscribers,
    DateTimeOffset Updated
);

public class GetPlayerConsumer : IMediatorConsumer<GetPlayer> {
    private readonly AppDbContext _db;

    public GetPlayerConsumer(AppDbContext db) {
        _db = db;
    }

    public async Task Consume(ConsumeContext<GetPlayer> context) {
        CancellationToken cancellationToken = context.CancellationToken;
        string playerUrl = context.Message.PlayerUrl;

        PlayerEntity? entity = await _db.Players.FindAsync([playerUrl], cancellationToken: cancellationToken);

        if (entity is null) {
            await context.RespondAsync(new GetPlayerNotFoundResult());
            return;
        }

        int subscribers =
            await _db.Subscriptions.CountAsync(s => s.PlayerUrl == playerUrl, cancellationToken);

        await context.RespondAsync(new GetPlayerResult(
            entity.PlayerUrl,
            entity.Fio,
            entity.Rating,
            entity.Position,
            subscribers,
            TimeZoneInfo.ConvertTime(entity.Updated, Constants.RussianTimeZone)
        ));
    }
}
