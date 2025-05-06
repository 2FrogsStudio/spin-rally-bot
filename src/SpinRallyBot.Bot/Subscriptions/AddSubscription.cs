namespace SpinRallyBot.Subscriptions;

public record AddSubscription(long ChatId, string PlayerUrl);

public class AddSubscriptionConsumer(AppDbContext db, IScopedMediator mediator) : IMediatorConsumer<AddSubscription> {
    public async Task Consume(ConsumeContext<AddSubscription> context) {
        if (context.Message is not {
                PlayerUrl: var playerUrl,
                ChatId: var chatId
            }) {
            return;
        }

        CancellationToken cancellationToken = context.CancellationToken;
        Response<GetPlayerResult, GetPlayerNotFoundResult> response = await mediator
            .CreateRequestClient<GetPlayer>()
            .GetResponse<GetPlayerResult, GetPlayerNotFoundResult>(new GetPlayer(playerUrl),
                cancellationToken);

        if (response.Is<GetPlayerNotFoundResult>(out _)) {
            throw new InvalidOperationException("Player not found by Url") {
                Data = { { "PlayerUrl", playerUrl } }
            };
        }

        if (!response.Is<GetPlayerResult>(out Response<GetPlayerResult>? result) || result.Message is not { } player) {
            throw new UnreachableException();
        }

        SubscriptionEntity entity =
            await db.Subscriptions.FindAsync([chatId, playerUrl], cancellationToken)
            ?? new SubscriptionEntity {
                ChatId = chatId,
                PlayerUrl = player.PlayerUrl,
                Player = await db.Players.FindAsync([playerUrl], cancellationToken) ??
                         throw new InvalidOperationException()
            };

        if (db.Entry(entity).State is EntityState.Detached) {
            db.Add(entity);
        }

        await db.SaveChangesAsync(cancellationToken);
    }
}
