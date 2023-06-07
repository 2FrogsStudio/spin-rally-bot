namespace SpinRallyBot.Events.CallbackReceivedConsumers;

public class CommandCallbackReceivedConsumer : IMediatorConsumer<CallbackReceived> {
    private readonly IScopedMediator _mediator;

    public CommandCallbackReceivedConsumer(IScopedMediator mediator) {
        _mediator = mediator;
    }

    public async Task Consume(ConsumeContext<CallbackReceived> context) {
        if (context.Message is not {
                Data: NavigationData.CommandData {
                    Command: var command,
                    Data: var data
                },
                MessageId: var messageId,
                ChatId: var chatId,
                ChatType: var chatType,
                UserId: var userId
            }
           ) {
            return;
        }

        var args = data?.Split(' ') ?? Array.Empty<string>();

        await _mediator.Publish(new CommandReceived(command, args, chatId, chatType, null, messageId, userId),
            context.CancellationToken);
    }
}