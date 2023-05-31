using System.Diagnostics;
using SpinRallyBot.Attributes;
using SpinRallyBot.PipelineStateMachine;

namespace SpinRallyBot.Events.CallbackReceivedConsumers;

public class FindPipelineCallbackReceivedConsumer : IMediatorConsumer<CallbackReceived> {
    private readonly ITelegramBotClient _botClient;
    private readonly IScopedMediator _mediator;

    public FindPipelineCallbackReceivedConsumer(IScopedMediator mediator, ITelegramBotClient botClient) {
        _mediator = mediator;
        _botClient = botClient;
    }

    public async Task Consume(ConsumeContext<CallbackReceived> context) {
        if (context.Message is not {
                NavigationData: NavigationData.PipelineData {
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

        var args = data?.Split(' ') ?? Array.Empty<string>();
        var cancellationToken = context.CancellationToken;

        switch (args) {
            case []:
                //input player pipeline
                var arg = Command.Find.GetAttributesOfType<CommandArgAttribute>()[0];
                await _mediator.Send(new SetPipelineData(userId, chatId, new PipelineData(Pipeline.Find)), cancellationToken);
                await _botClient.SendTextMessageAsync(
                    chatId,
                    arg.Name,
                    replyMarkup: new ForceReplyMarkup {
                        Selective = true,
                        InputFieldPlaceholder = arg.Description
                    }, cancellationToken: cancellationToken);
                return;
            case [_]:
                await _mediator.Publish(new CommandReceived(Command.Find, args, chatId, chatType, null, messageId, userId), cancellationToken);
                break;
            default:
                throw new UnreachableException();
        }
    }
}