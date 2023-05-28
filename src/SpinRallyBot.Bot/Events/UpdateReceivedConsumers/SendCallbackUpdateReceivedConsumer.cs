namespace SpinRallyBot.Events.UpdateReceivedConsumers;

public class SendCallbackUpdateReceivedConsumer : IConsumer<UpdateReceived>, IMediatorConsumer {
    private readonly ITelegramBotClient _botClient;
    private readonly IScopedMediator _mediator;

    public SendCallbackUpdateReceivedConsumer(IScopedMediator mediator, ITelegramBotClient botClient) {
        _mediator = mediator;
        _botClient = botClient;
    }

    public async Task Consume(ConsumeContext<UpdateReceived> context) {
        var update = context.Message.Update;
        if (update is not {
                Type: UpdateType.CallbackQuery,
                CallbackQuery.Id: var callbackId
            }) {
            return;
        }

        var cancellationToken = context.CancellationToken;

        await _botClient.AnswerCallbackQueryAsync(callbackId, cancellationToken: context.CancellationToken);

        await _mediator.Publish(new CallbackWithCommandGotten {
            CallbackQuery = update.CallbackQuery!
        }, cancellationToken);
    }
}