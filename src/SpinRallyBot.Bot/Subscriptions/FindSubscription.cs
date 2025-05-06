namespace SpinRallyBot.Subscriptions;

public record FindSubscription(long ChatId, string PlayerUrl);

public record SubscriptionNotFound;

public record SubscriptionFound;

public class FindSubscriptionConsumer(AppDbContext db) : IMediatorConsumer<FindSubscription> {
    public async Task Consume(ConsumeContext<FindSubscription> context) {
        CancellationToken cancellationToken = context.CancellationToken;

        SubscriptionEntity? subscription =
            await db.Subscriptions.FindAsync([context.Message.ChatId, context.Message.PlayerUrl],
                cancellationToken);

        if (subscription is not null) {
            await context.RespondAsync(new SubscriptionFound());
        } else {
            await context.RespondAsync(new SubscriptionNotFound());
        }
    }
}
