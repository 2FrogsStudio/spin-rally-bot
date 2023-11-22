namespace SpinRallyBot.Events.UpdateReceivedConsumers;

public class PublishCallbackQueryReceivedUpdateReceivedConsumer(IScopedMediator mediator, ITelegramBotClient botClient)
    : IConsumer<UpdateReceived> {
    public async Task Consume(ConsumeContext<UpdateReceived> context) {
        var update = context.Message.Update;
        if (update is not {
                Type: UpdateType.CallbackQuery,
                CallbackQuery: {
                    Id: var callbackId,
                    Message: {
                        MessageId: var messageId,
                        Chat: {
                            Id: var chatId,
                            Type: var chatType
                        }
                    },
                    From.Id: var userId,
                    Data: { } data
                }
            }) {
            return;
        }

        var cancellationToken = context.CancellationToken;

        var navigationData = JsonSerializer.Deserialize<NavigationData>(data)!;
        try {
            await mediator.Publish(new CallbackReceived(
                Data: navigationData,
                messageId,
                ChatId: chatId,
                ChatType: chatType,
                userId,
                context.Message.IsBotAdmin
            ), cancellationToken);
        } finally {
            await botClient.AnswerCallbackQueryAsync(callbackId, cancellationToken: cancellationToken);
        }
    }
}
