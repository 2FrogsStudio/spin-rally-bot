namespace SpinRallyBot.Subscriptions;

public record GetSubscriptions(long ChatId) { }

public class GetSubscriptionsConsumer : IMediatorConsumer<GetSubscriptions> {
    private readonly AppDbContext _db;

    public GetSubscriptionsConsumer(AppDbContext db) {
        _db = db;
    }

    public async Task Consume(ConsumeContext<GetSubscriptions> context) {
        var cancellationToken = context.CancellationToken;

        var subscriptions = await _db.Subscriptions
            .Where(s => s.ChatId == context.Message.ChatId)
            .ToArrayAsync(cancellationToken);

        await context.RespondAsync(subscriptions);
    }
}