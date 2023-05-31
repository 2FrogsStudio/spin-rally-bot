using System.Diagnostics;
using SpinRallyBot.Subscriptions;

namespace SpinRallyBot.Events.CallbackReceivedConsumers;

public class SubscriptionControlActionCallbackReceivedConsumer : IMediatorConsumer<CallbackReceived> {
    private readonly IScopedMediator _mediator;

    
    public SubscriptionControlActionCallbackReceivedConsumer(IScopedMediator mediator) {
        _mediator = mediator;
    }

    public async Task Consume(ConsumeContext<CallbackReceived> context) {
        if (context.Message is not {
                NavigationData: NavigationData.ActionData {
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

        await _mediator.Publish(new CallbackReceived(new NavigationData.CommandData(Command.Start), messageId, chatId, chatType, userId), cancellationToken);
    }
}