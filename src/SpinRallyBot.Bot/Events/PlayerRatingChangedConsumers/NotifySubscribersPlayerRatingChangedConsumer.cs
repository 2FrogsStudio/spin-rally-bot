namespace SpinRallyBot.Events.PlayerRatingChangedConsumers;

public class NotifySubscribersPlayerRatingChangedConsumer : IConsumer<PlayerRatingChanged> {
    private readonly ITelegramBotClient _bot;
    private readonly AppDbContext _db;
    private readonly IScopedMediator _mediator;

    public NotifySubscribersPlayerRatingChangedConsumer(AppDbContext db, ITelegramBotClient bot,
        IScopedMediator mediator) {
        _db = db;
        _bot = bot;
        _mediator = mediator;
    }

    public async Task Consume(ConsumeContext<PlayerRatingChanged> context) {
        var subscriptions = await _db.Subscriptions
            .Where(s => s.PlayerUrl == context.Message.PlayerUrl)
            .Select(s => new { chatId = s.ChatId, fio = s.Player.Fio, playerUrl = s.PlayerUrl })
            .ToArrayAsync(context.CancellationToken);

        var tasks = subscriptions
            .Select(s => SendNotification(s.chatId, s.fio, s.playerUrl, context.Message, context.CancellationToken));

        await Task.WhenAll(tasks);
    }

    private async Task SendNotification(long chatId, string fio, string playerUrl, PlayerRatingChanged changed,
        CancellationToken cancellationToken) {
        var ratingDelta = changed.NewRating - changed.OldRating;
        var positionDelta = changed.NewPosition - changed.OldPosition;
        var text =
            $"{(ratingDelta > 0 ? "📈" : "📉")} Рейтинг обновлен ".ToEscapedMarkdownV2() + '\n' +
            $"Участник: {fio}".ToEscapedMarkdownV2() + '\n' +
            $"Рейтинг: {changed.NewRating}({(ratingDelta > 0 ? "+" : null)}{ratingDelta})"
                .ToEscapedMarkdownV2() + '\n' +
            $"Рейтинг: {changed.NewPosition}({(positionDelta > 0 ? "+" : null)}{positionDelta})"
                .ToEscapedMarkdownV2();

        var buttons = new List<InlineKeyboardButton>();

        var findSubscriptionResponse = await _mediator
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

        await _bot.SendTextMessageAsync(
            chatId,
            text,
            parseMode: ParseMode.MarkdownV2,
            replyMarkup: new InlineKeyboardMarkup(buttons.Split(1)),
            cancellationToken: cancellationToken
        );
    }
}