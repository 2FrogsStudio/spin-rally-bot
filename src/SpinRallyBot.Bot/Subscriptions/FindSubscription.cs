namespace SpinRallyBot.Subscriptions;

public record FindSubscription(long ChatId, string PlayerUrl);

public class FindSubscriptionConsumer : IMediatorConsumer<FindSubscription> {
    private readonly AppDbContext _db;

    public FindSubscriptionConsumer(AppDbContext db) {
        _db = db;
    }

    public async Task Consume(ConsumeContext<FindSubscription> context) {
        var cancellationToken = context.CancellationToken;

        var subscription =
            await _db.Subscriptions.FindAsync(new object[] { context.Message.ChatId, context.Message.PlayerUrl },
                cancellationToken);

        if (subscription is null) {
            await context.RespondAsync(new SubscriptionNotFound());
            return;
        }

        await context.RespondAsync(new SubscriptionResult(subscription.Player.Fio, subscription.PlayerUrl));
    }
}