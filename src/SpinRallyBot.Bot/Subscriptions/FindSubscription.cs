namespace SpinRallyBot.Subscriptions;

public record FindSubscription(long ChatId, string PlayerUrl) { }

public record SubscriptionNotFound;

public class FindSubscriptionConsumer : IMediatorConsumer<FindSubscription> {
    private readonly AppDbContext _db;

    public FindSubscriptionConsumer(AppDbContext db) {
        _db = db;
    }

    public async Task Consume(ConsumeContext<FindSubscription> context) {
        var cancellationToken = context.CancellationToken;

        var subscription = await _db.Subscriptions.FindAsync(context.Message.ChatId, context.Message.PlayerUrl);

        if (subscription is null) {
            await context.RespondAsync(new SubscriptionNotFound());
        } else {
            await context.RespondAsync(subscription);
        }
    }
}