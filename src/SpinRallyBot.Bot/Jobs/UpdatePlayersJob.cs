using MassTransit.Scheduling;

namespace SpinRallyBot.Jobs;

public record UpdatePlayersJob;

public class UpdatePlayersJobSchedule : DefaultRecurringSchedule {
    public UpdatePlayersJobSchedule(bool isDevelopment) {
        TimeZoneId = TimeZoneInfo.Utc.Id;
        // todo: pass through configuration
        CronExpression = "0 0 8-20/4 1/1 * ? *"; // every 4th hour from 8 through 20
        MisfirePolicy = isDevelopment ? MissedEventPolicy.Skip : MissedEventPolicy.Default;
    }
}

public class UpdatePlayersJobConsumer : IConsumer<UpdatePlayersJob> {
    private readonly ITelegramBotClient _bot;
    private readonly AppDbContext _db;
    private readonly IScopedMediator _mediator;

    public UpdatePlayersJobConsumer(AppDbContext db, IScopedMediator mediator, ITelegramBotClient bot) {
        _db = db;
        _mediator = mediator;
        _bot = bot;
    }

    public async Task Consume(ConsumeContext<UpdatePlayersJob> context) {
        var cancellationToken = context.CancellationToken;
        var playerUrls = await _db.Players
            // update all players to get actual info in find dialog too 
            // .Where(p => p.Subscriptions.Count > 0)
            .Select(p => p.PlayerUrl)
            .ToArrayAsync(cancellationToken);

        var exceptions = new List<Exception>();
        var chatId = context.Headers.Get<uint>("ResponseChatId");
        var messageId = context.Headers.Get<int>("UpdateMessageId");

        for (var index = 0; index < playerUrls.Length; index++) {
            var playerUrl = playerUrls[index];
            try {
                await _mediator.Send(new UpdatePlayer(playerUrl, true), cancellationToken);
                await UpdateProgressMessage(chatId, messageId, index + 1, playerUrls.Length, cancellationToken);
            } catch (Exception ex) {
                exceptions.Add(ex);
            }
        }

        try {
            await FinishedProgressNotification(chatId, messageId, exceptions.Count > 0, cancellationToken);
        } catch (Exception ex) {
            exceptions.Add(ex);
        }

        if (exceptions.Count > 0) {
            throw new AggregateException("Encountered errors while trying to update players.", exceptions);
        }
    }

    private async Task UpdateProgressMessage(uint? chatId, int? messageId, int progress, int count,
        CancellationToken cancellationToken) {
        if (chatId is null || messageId is null) {
            return;
        }

        var totalProgress = Math.Round((float)progress / count * 100, 0);
        await _bot.EditMessageTextAsync(chatId.Value, messageId.Value,
            $"⏳Обновление {totalProgress}%..".ToEscapedMarkdownV2(), ParseMode.MarkdownV2,
            cancellationToken: cancellationToken);
    }

    private async Task FinishedProgressNotification(uint? chatId, int? messageId, bool failed,
        CancellationToken cancellationToken) {
        if (chatId is null || messageId is null) {
            return;
        }

        var text = failed
            ? "🚨 Ошибка при обновлении рейтинга, проверьте логи"
            : "✅ Обновление завершено";
        await _bot.EditMessageTextAsync(chatId.Value, messageId.Value, text,
            cancellationToken: cancellationToken);
    }
}

public class UpdatePlayersJobConsumerDefinition : ConsumerDefinition<UpdatePlayersJobConsumer> {
    public UpdatePlayersJobConsumerDefinition() {
        ConcurrentMessageLimit = 1;
    }
}