namespace SpinRallyBot.Subscriptions;

public class RemoveSubscriptionConsumer : IMediatorConsumer<RemoveSubscription> {
    private readonly AppDbContext _db;

    public RemoveSubscriptionConsumer(AppDbContext db) {
        _db = db;
    }

    public async Task Consume(ConsumeContext<RemoveSubscription> context) {
        if (context.Message is not {
                PlayerUrl: var playerUrl,
                ChatId: var chatId
            }) {
            return;
        }

        var cancellationToken = context.CancellationToken;

        var entity = await _db.Subscriptions.FindAsync(new object[] {
            chatId, playerUrl
        }, cancellationToken);
        if (entity is null) {
            return;
        }

        _db.Remove(entity);
        await _db.SaveChangesAsync(cancellationToken);
    }
}

public record RemoveSubscription(long ChatId, string PlayerUrl);
