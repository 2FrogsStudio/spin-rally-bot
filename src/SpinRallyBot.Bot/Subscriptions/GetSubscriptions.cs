namespace SpinRallyBot.Subscriptions;

public record GetSubscriptions(long ChatId);

public record GetSubscriptionsResult((string Fio, float Rating, string PlayerUrl)[] Subscriptions);

public class GetSubscriptionsConsumer : IMediatorConsumer<GetSubscriptions> {
    private readonly AppDbContext _db;

    public GetSubscriptionsConsumer(AppDbContext db) {
        _db = db;
    }

    public async Task Consume(ConsumeContext<GetSubscriptions> context) {
        var cancellationToken = context.CancellationToken;

        var subscriptionEntities = await _db.Subscriptions
            .Where(s => s.ChatId == context.Message.ChatId)
            .OrderBy(s => s.Player.Fio)
            .Select(s => new { s.Player.Fio, s.Player.Rating, s.PlayerUrl })
            .ToArrayAsync(cancellationToken);

        await context.RespondAsync(new GetSubscriptionsResult(
            subscriptionEntities
                .Select(s => (s.Fio, s.Rating, s.PlayerUrl))
                .ToArray()
        ));
    }
}