namespace SpinRallyBot.Subscriptions;

public record GetSubscriptionsByChatId(long ChatId);

public record GetSubscriptionsByChatIdResult((string Fio, float Rating, string PlayerUrl)[] Subscriptions);

public class GetSubscriptionsByChatIdConsumer : IMediatorConsumer<GetSubscriptionsByChatId> {
    private readonly AppDbContext _db;

    public GetSubscriptionsByChatIdConsumer(AppDbContext db) {
        _db = db;
    }

    public async Task Consume(ConsumeContext<GetSubscriptionsByChatId> context) {
        var cancellationToken = context.CancellationToken;

        var subscriptionEntities = await _db.Subscriptions
            .Where(s => s.ChatId == context.Message.ChatId)
            .OrderBy(s => s.Player.Fio)
            .Select(s => new { s.Player.Fio, s.Player.Rating, s.PlayerUrl })
            .ToArrayAsync(cancellationToken);

        await context.RespondAsync(new GetSubscriptionsByChatIdResult(
            subscriptionEntities
                .Select(s => (s.Fio, s.Rating, s.PlayerUrl))
                .ToArray()
        ));
    }
}