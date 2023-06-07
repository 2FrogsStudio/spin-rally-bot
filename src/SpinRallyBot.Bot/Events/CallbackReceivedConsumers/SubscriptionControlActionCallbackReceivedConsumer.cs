namespace SpinRallyBot.Events.CallbackReceivedConsumers;

public class SubscriptionControlActionCallbackReceivedConsumer : IMediatorConsumer<CallbackReceived> {
    private readonly IScopedMediator _mediator;


    public SubscriptionControlActionCallbackReceivedConsumer(IScopedMediator mediator) {
        _mediator = mediator;
    }

    public async Task Consume(ConsumeContext<CallbackReceived> context) {
        if (context.Message is not {
                Data: NavigationData.ActionData {
                    Action: var action and (Actions.Subscribe or Actions.Unsubscribe),
                    Data: { } playerUrl
                },
                ChatId: var chatId,
                MessageId: var messageId,
                ChatType: var chatType,
                UserId: var userId
            }) {
            return;
        }

        var cancellationToken = context.CancellationToken;

        switch (action) {
            case Actions.Subscribe:
                await _mediator.Send(new AddSubscription(chatId, playerUrl), cancellationToken);
                break;
            case Actions.Unsubscribe:
                await _mediator.Send(new RemoveSubscription(chatId, playerUrl), cancellationToken);
                break;
            default:
                throw new UnreachableException();
        }

        await _mediator.Send(new CallbackReceived(
            new NavigationData.CommandData(Command.Find, playerUrl),
            messageId,
            chatId,
            UserId: userId,
            ChatType: chatType
        ), cancellationToken);


        // var popResponse = await _mediator
        //     .CreateRequestClient<PopBackNavigation>()
        //     .GetResponse<BackNavigation, EmptyNavigation>(new PopBackNavigation(UserId: userId, ChatId: chatId), cancellationToken);
        //
        // if (popResponse.Is<BackNavigation>(out var popResult) && popResult.Message is { } pop) {
        // }
    }
}