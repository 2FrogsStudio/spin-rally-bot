using SpinRallyBot.Attributes;

namespace SpinRallyBot.Events.CallbackReceivedConsumers;

public class FindPipelineCallbackReceivedConsumer(IScopedMediator mediator, ITelegramBotClient botClient)
    : IMediatorConsumer<CallbackReceived> {
    private readonly ITelegramBotClient _botClient = botClient;
    private readonly IScopedMediator _mediator = mediator;

    public async Task Consume(ConsumeContext<CallbackReceived> context) {
        if (context.Message is not {
                Data: NavigationData.PipelineData {
                    Pipeline: Pipeline.Find,
                    Data: var data
                },
                MessageId: var messageId,
                ChatId: var chatId,
                ChatType: var chatType,
                UserId: var userId
            }) {
            return;
        }

        // ReSharper disable once NullCoalescingConditionIsAlwaysNotNullAccordingToAPIContract
        string[] args = data ?? [];
        CancellationToken cancellationToken = context.CancellationToken;

        switch (args) {
            case []:
                //input player pipeline
                CommandArgAttribute arg = Command.Find.GetAttributesOfType<CommandArgAttribute>()[0];
                await _mediator.Send(new SetPipelineData(userId, chatId, new PipelineData(Pipeline.Find)),
                    cancellationToken);
                await _botClient.SendMessage(
                    chatId,
                    arg.Name,
                    disableNotification: true,
                    replyMarkup: new ForceReplyMarkup {
                        Selective = true,
                        InputFieldPlaceholder = arg.Description
                    }, cancellationToken: cancellationToken);
                return;
            case [_]:
                await _mediator.Publish(
                    new CommandReceived(
                        Command.Find,
                        args,
                        chatId,
                        chatType,
                        null,
                        messageId,
                        userId,
                        context.Message.IsBotAdmin
                    ), cancellationToken);
                break;
            default:
                throw new UnreachableException();
        }
    }
}
