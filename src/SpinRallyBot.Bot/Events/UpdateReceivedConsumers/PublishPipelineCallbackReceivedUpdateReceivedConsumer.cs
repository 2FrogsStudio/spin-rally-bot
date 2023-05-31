using System.Diagnostics;
using SpinRallyBot.BackNavigations;
using SpinRallyBot.PipelineStateMachine;

namespace SpinRallyBot.Events.UpdateReceivedConsumers;

public class PublishPipelineCallbackReceivedUpdateReceivedConsumer : IMediatorConsumer<UpdateReceived> {
    private readonly IScopedMediator _mediator;

    public PublishPipelineCallbackReceivedUpdateReceivedConsumer(IScopedMediator mediator) {
        _mediator = mediator;
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

        // ReSharper disable once ConditionalAccessQualifierIsNonNullableAccordingToAPIContract
        args = args?.Append(messageText).ToArray() ?? new[] { messageText };

        var data = string.Join(' ', args);
        var navigationData = new NavigationData.PipelineData(pipeline, data);
        await _mediator.Send(new PushBackNavigation(userId, chatId, Guid.NewGuid(), "≡ Список", navigationData), cancellationToken);
        try {
            await _mediator.Publish(new CallbackReceived(
                MessageId: null,
                Data: navigationData,
                ChatId: chatId,
                ChatType: chatType,
                UserId: userId
            ), cancellationToken);
        } finally {
            await _mediator.Send(new RemovePipelineState(userId, chatId), cancellationToken);
        }
    }
}