namespace SpinRallyBot.Events.UpdateReceivedConsumers;

public class PublishPipelineCallbackReceivedUpdateReceivedConsumer(IScopedMediator mediator)
    : IConsumer<UpdateReceived> {
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

        var result = await mediator
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

        var navigationData = new NavigationData.PipelineData(pipeline, args);
        await mediator.Send(new PushBackNavigation(userId, chatId, Guid.NewGuid(), "≡ Список", navigationData),
            cancellationToken);
        try {
            await mediator.Publish(new CallbackReceived(
                navigationData,
                null,
                chatId,
                chatType,
                userId,
                context.Message.IsBotAdmin
            ), cancellationToken);
        } finally {
            await mediator.Send(new RemovePipelineState(userId, chatId), cancellationToken);
        }
    }
}
