namespace SpinRallyBot.Events.PlayerRatingChangedConsumers;

public class NotifySubscribersPlayerRatingChangedConsumer : IConsumer<PlayerRatingChanged> {
    private readonly ITelegramBotClient _bot;
    private readonly AppDbContext _db;

    public NotifySubscribersPlayerRatingChangedConsumer(AppDbContext db, ITelegramBotClient bot) {
        _db = db;
        _bot = bot;
    }

    public async Task Consume(ConsumeContext<PlayerRatingChanged> context) {
        var subscriptions = await _db.Subscriptions
            .Where(s => s.PlayerUrl == context.Message.PlayerUrl)
            .Select(s => new { chatId = s.ChatId, fio = s.Player.Fio })
            .ToArrayAsync(context.CancellationToken);

        var tasks = subscriptions
            .Select(s => SendNotification(s.chatId, s.fio, context.Message, context.CancellationToken));

        await Task.WhenAll(tasks);
    }

    private async Task SendNotification(long chatId, string fio, PlayerRatingChanged changed,
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

        await _bot.SendTextMessageAsync(
            chatId,
            text,
            parseMode: ParseMode.MarkdownV2,
            cancellationToken: cancellationToken
        );
    }
}