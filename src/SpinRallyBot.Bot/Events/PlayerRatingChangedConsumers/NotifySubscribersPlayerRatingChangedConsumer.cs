using Telegram.Bot.Exceptions;

namespace SpinRallyBot.Events.PlayerRatingChangedConsumers;

public class NotifySubscribersPlayerRatingChangedConsumer : IConsumer<PlayerRatingChanged> {
    private readonly ITelegramBotClient _bot;
    private readonly AppDbContext _db;
    private readonly ILogger<NotifySubscribersPlayerRatingChangedConsumer> _logger;
    private readonly IScopedMediator _mediator;

    public NotifySubscribersPlayerRatingChangedConsumer(AppDbContext db,
        ITelegramBotClient bot,
        IScopedMediator mediator,
        ILogger<NotifySubscribersPlayerRatingChangedConsumer> logger) {
        _db = db;
        _bot = bot;
        _mediator = mediator;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<PlayerRatingChanged> context) {
        var subscriptions = await _db.Subscriptions
            .Where(s => s.PlayerUrl == context.Message.PlayerUrl)
            .Select(s => new { chatId = s.ChatId, playerUrl = s.PlayerUrl })
            .ToArrayAsync(context.CancellationToken);

        var exceptions = new List<Exception>();

        foreach (var subscription in subscriptions) {
            try {
                await SendNotification(subscription.chatId, subscription.playerUrl, context.Message,
                    context.CancellationToken);
            } catch (ApiRequestException ex) when (ex.ErrorCode is 403 &&
                                                   ex.Message is "Forbidden: bot was blocked by the user") {
                await RemoveBannedSubscription(subscription.chatId, context.CancellationToken);
            } catch (Exception ex) {
                exceptions.Add(ex);
            }
        }

        if (exceptions.Count > 0) {
            throw new AggregateException("Encountered errors while trying to update players.", exceptions);
        }
    }

    private async Task RemoveBannedSubscription(long chatId, CancellationToken cancellationToken) {
        SubscriptionEntity[] entities =
            await _db.Subscriptions.Where(s => s.ChatId == chatId).ToArrayAsync(cancellationToken);
        _db.Subscriptions.RemoveRange(entities);
        await _db.SaveChangesAsync(cancellationToken);
    }

    private async Task SendNotification(long chatId, string playerUrl, PlayerRatingChanged changed,
        CancellationToken cancellationToken) {
        Response<GetPlayerResult> result = await _mediator
            .CreateRequestClient<GetPlayer>()
            .GetResponse<GetPlayerResult>(new GetPlayer(playerUrl),
                cancellationToken);

        GetPlayerResult player = result.Message;

        bool isIncreased = player.Rating >= changed.OldRating;

        string ratingDelta =
            isIncreased
                ? $"{changed.OldRating:F2} + {player.Rating - changed.OldRating:F2} → {player.Rating:F2}"
                : $"{changed.OldRating:F2} - {changed.OldRating - player.Rating:F2} → {player.Rating:F2}";

        string positionDelta =
            player.Position >= changed.OldPosition
                ? player.Position == changed.OldPosition
                    ? $"{player.Position}"
                    : $"{changed.OldPosition} + {player.Position - changed.OldPosition} → {player.Position}"
                : $"{changed.OldPosition} - {changed.OldPosition - player.Position} → {player.Position}";

        _logger.LogInformation(
            "Player's rating updated: OldRating:{OldRating} NewRating:{NewRating} OldPosition:{OldPosition} NewPosition:{NewPosition}",
            changed.OldRating, player.Rating, changed.OldPosition, player.Position);

        string text =
            $"{(isIncreased ? "🚀" : "🔻")} Рейтинг обновлен ".ToEscapedMarkdownV2() + '\n' +
            $"{player.Fio}".ToEscapedMarkdownV2() + "\n" +
            $"Рейтинг: {ratingDelta}".ToEscapedMarkdownV2() + '\n' +
            $"Позиция: {positionDelta}".ToEscapedMarkdownV2() + '\n' +
            $"Подписчиков: {player.Subscribers}".ToEscapedMarkdownV2() + "\n" +
            $"Обновлено: {player.Updated:dd.MM.yyyy H:mm} (МСК)".ToEscapedMarkdownV2() + "\n" +
            $"{Constants.RttwUrl}{player.PlayerUrl}".ToEscapedMarkdownV2();

        var buttons = new List<InlineKeyboardButton>();

        Response<SubscriptionFound, SubscriptionNotFound> findSubscriptionResponse = await _mediator
            .CreateRequestClient<FindSubscription>()
            .GetResponse<SubscriptionFound, SubscriptionNotFound>(new FindSubscription(chatId, playerUrl),
                cancellationToken);
        if (findSubscriptionResponse.Is<SubscriptionFound>(out _)) {
            buttons.Add(new InlineKeyboardButton("Отписаться") {
                CallbackData =
                    JsonSerializer.Serialize(
                        new NavigationData.ActionData(Actions.Unsubscribe, playerUrl, true))
            });
        }

        if (findSubscriptionResponse.Is<SubscriptionNotFound>(out _)) {
            buttons.Add(new InlineKeyboardButton("Подписаться") {
                CallbackData =
                    JsonSerializer.Serialize(new NavigationData.ActionData(Actions.Subscribe, playerUrl, true))
            });
        }

        buttons.Add(new InlineKeyboardButton("↩︎ Меню") {
            CallbackData = JsonSerializer.Serialize(new NavigationData.CommandData(Command.Start, newThread: true))
        });

        await _bot.SendMessage(
            chatId,
            text,
            parseMode: ParseMode.MarkdownV2,
            replyMarkup: new InlineKeyboardMarkup(buttons.Split(1)),
            cancellationToken: cancellationToken
        );
    }
}
