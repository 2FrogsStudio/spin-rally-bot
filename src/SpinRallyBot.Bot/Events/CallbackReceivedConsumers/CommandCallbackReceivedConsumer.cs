namespace SpinRallyBot.Events.CallbackReceivedConsumers;

public class CommandCallbackReceivedConsumer(IScopedMediator mediator) : IMediatorConsumer<CallbackReceived> {
    public async Task Consume(ConsumeContext<CallbackReceived> context) {
        if (context.Message is not {
                Data: NavigationData.CommandData {
                    Command: var command,
                    Data: var data,
                    NewThread: var newThread
                },
                MessageId: var messageId,
                ChatId: var chatId,
                ChatType: var chatType,
                UserId: var userId
            }
           ) {
            return;
        }

        // ReSharper disable once ConditionalAccessQualifierIsNonNullableAccordingToAPIContract
        string[] args = data?.Split(' ') ?? [];

        await mediator.Publish(
            new CommandReceived(
                command,
                args,
                chatId,
                chatType,
                null,
                newThread ? null : messageId,
                userId,
                context.Message.IsBotAdmin
            ), context.CancellationToken);
    }
}
