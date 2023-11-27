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

public class UpdatePlayersJobConsumer(AppDbContext db, IScopedMediator mediator, ITelegramBotClient bot)
    : IConsumer<UpdatePlayersJob> {
    public async Task Consume(ConsumeContext<UpdatePlayersJob> context) {
        var cancellationToken = context.CancellationToken;
        var playerUrls = await db.Players
            // update all players to get actual info in find dialog too 
            // .Where(p => p.Subscriptions.Count > 0)
            .Select(p => p.PlayerUrl)
            .ToArrayAsync(cancellationToken);

        var exceptions = new List<Exception>();

        foreach (var playerUrl in playerUrls)
            try {
                await mediator.Send(new UpdatePlayer(
                    playerUrl,
                    true
                ), cancellationToken);
            } catch (Exception ex) {
                exceptions.Add(ex);
            }

        try {
            await SendFinishedNotification(context, exceptions.Count > 0, cancellationToken);
        } catch (Exception ex) {
            exceptions.Add(ex);
        }

        if (exceptions.Count > 0) {
            throw new AggregateException("Encountered errors while trying to update players.", exceptions);
        }
    }

    private async Task SendFinishedNotification(MessageContext context, bool failed,
        CancellationToken cancellationToken) {
        var chatId = context.Headers.Get<uint>("RespondChatId");
        if (chatId is not null) {
            var replyToMessageId = context.Headers.Get<int>("RespondReplyToMessageId");
            if (failed) {
                await bot.SendTextMessageAsync(chatId, "🚨 Update failed, check logs",
                    replyToMessageId: replyToMessageId, cancellationToken: cancellationToken);
            } else {
                await bot.SendTextMessageAsync(chatId, "✅ Update finished",
                    replyToMessageId: replyToMessageId, cancellationToken: cancellationToken);
            }
        }
    }
}

public class UpdatePlayersJobConsumerDefinition : ConsumerDefinition<UpdatePlayersJobConsumer> {
    public UpdatePlayersJobConsumerDefinition() {
        ConcurrentMessageLimit = 1;
    }
}