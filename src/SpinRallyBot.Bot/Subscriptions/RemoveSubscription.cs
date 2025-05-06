namespace SpinRallyBot.Subscriptions;

public class RemoveSubscriptionConsumer(AppDbContext db) : IMediatorConsumer<RemoveSubscription> {
    public async Task Consume(ConsumeContext<RemoveSubscription> context) {
        if (context.Message is not {
                PlayerUrl: var playerUrl,
                ChatId: var chatId
            }) {
            return;
        }

        CancellationToken cancellationToken = context.CancellationToken;

        SubscriptionEntity? entity = await db.Subscriptions.FindAsync([
            chatId, playerUrl
        ], cancellationToken);
        if (entity is null) {
            return;
        }

        db.Remove(entity);
        await db.SaveChangesAsync(cancellationToken);
    }
}

public record RemoveSubscription(long ChatId, string PlayerUrl);
