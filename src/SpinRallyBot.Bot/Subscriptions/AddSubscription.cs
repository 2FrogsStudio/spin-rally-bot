using System.Diagnostics;
using SpinRallyBot.Models;
using SpinRallyBot.Queries;

namespace SpinRallyBot.Subscriptions;

public record AddSubscription(long ChatId, string PlayerUrl);

public class AddSubscriptionConsumer : IMediatorConsumer<AddSubscription> {
    private readonly AppDbContext _db;
    private readonly IScopedMediator _mediator;

    public AddSubscriptionConsumer(AppDbContext db, IScopedMediator mediator) {
        _db = db;
        _mediator = mediator;
    }

    public async Task Consume(ConsumeContext<AddSubscription> context) {
        if (context.Message is not {
                PlayerUrl: var playerUrl,
                ChatId: var chatId
            }) {
            return;
        }

        var cancellationToken = context.CancellationToken;

        var response = await _mediator.CreateRequestClient<GetCachedPlayerInfo>()
            .GetResponse<PlayerInfo, PlayerInfoNotFound>(new GetCachedPlayerInfo(playerUrl), cancellationToken);

        if (response.Is<PlayerInfoNotFound>(out _)) {
            throw new InvalidOperationException("Player not found by Url") {
                Data = { { "PlayerUrl", playerUrl } }
            };
        }

        if (!response.Is<PlayerInfo>(out var result) || result.Message is not Player player) {
            throw new UnreachableException();
        }

        var entity = await _db.Subscriptions.FindAsync(chatId, playerUrl);
        if (entity is not null) {
            return;
        }

        entity ??= new Subscription {
            ChatId = chatId,
            Fio = player.Fio,
            PlayerUrl = player.PlayerUrl
        };
        _db.Subscriptions.Add(entity);

        await _db.SaveChangesAsync(cancellationToken);
    }
}