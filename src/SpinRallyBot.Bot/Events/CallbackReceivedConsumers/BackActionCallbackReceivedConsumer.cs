namespace SpinRallyBot.Events.CallbackReceivedConsumers;

public class BackActionCallbackReceivedConsumer : IMediatorConsumer<CallbackReceived> {
    private readonly IScopedMediator _mediator;

    public BackActionCallbackReceivedConsumer(IScopedMediator mediator) {
        _mediator = mediator;
    }

    public async Task Consume(ConsumeContext<CallbackReceived> context) {
        if (context.Message is not {
                Data: NavigationData.BackData {
                    Guid: var guid
                },
                ChatId: var chatId,
                MessageId: var messageId,
                ChatType: var chatType,
                UserId: var userId
            }) {
            return;
        }

        CancellationToken cancellationToken = context.CancellationToken;

        Response<BackNavigation, EmptyNavigation> response = await _mediator
            .CreateRequestClient<PopBackNavigation>()
            .GetResponse<BackNavigation, EmptyNavigation>(new PopBackNavigation(userId, chatId, guid),
                cancellationToken);

        if (response.Is<BackNavigation>(out Response<BackNavigation>? backResponse) &&
            backResponse.Message.Data is { } data) {
            await _mediator.Publish(new CallbackReceived(
                data,
                messageId,
                chatId,
                chatType,
                userId,
                context.Message.IsBotAdmin
            ), cancellationToken);
        }
    }
}
