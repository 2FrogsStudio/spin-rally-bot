using System.Diagnostics;
using SpinRallyBot.BackNavigations;
using SpinRallyBot.PipelineStateMachine;

namespace SpinRallyBot.Events.UpdateReceivedConsumers;

public class PublishPipelineCallbackReceivedUpdateReceivedConsumer : IMediatorConsumer<UpdateReceived> {
    private readonly IScopedMediator _mediator;
    private readonly ITelegramBotClient _botClient;

    public PublishPipelineCallbackReceivedUpdateReceivedConsumer(IScopedMediator mediator, ITelegramBotClient botClient) {
        _mediator = mediator;
        _botClient = botClient;
    }

    public async Task Consume(ConsumeContext<UpdateReceived> context) {
        var update = context.Message.Update;
        var cancellationToken = context.CancellationToken;

        if (update is not {
                Message : {
                    Text: { } messageText,
                    From.Id: var userId,
                    Chat: {
                        Type: var chatType,
                        Id: var chatId
                    }
                }
            }
            || messageText.StartsWith('/')) {
            return;
        }

        var result = await _mediator
            .CreateRequestClient<GetPipelineData>()
            .GetResponse<PipelineData, NoPipelineStateResult>(new GetPipelineData(userId, chatId), cancellationToken);

        if (result.Is<NoPipelineStateResult>(out _)) {
            return;
        }

        if (!result.Is<PipelineData>(out var response)
            || response is not { Message: { Pipeline: var pipeline, Args: var args } }) {
            throw new UnreachableException();
        }

        args = args is null
            ? new[] { messageText }
            : args.Append(messageText).ToArray();

        var data = string.Join(' ', args);
        var navigationData = new NavigationData.PipelineData(pipeline, data);
        await _mediator.Send(new PushBackNavigation(userId, chatId, Guid.NewGuid(), "↩︎ Список", navigationData), cancellationToken);
        // await _botClient.DeleteMessageAsync(chatId, messageId, cancellationToken);
        try {
            await _mediator.Publish(new CallbackReceived(
                MessageId: null,
                NavigationData: navigationData,
                ChatId: chatId,
                ChatType: chatType,
                UserId: userId
            ), cancellationToken);
        } finally {
            await _mediator.Send(new RemovePipelineState(userId, chatId), cancellationToken);
        }
    }
}