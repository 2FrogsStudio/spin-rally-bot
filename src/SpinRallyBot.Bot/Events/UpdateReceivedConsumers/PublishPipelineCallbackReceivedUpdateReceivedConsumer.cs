using System.Diagnostics;
using SpinRallyBot.PipelineStateMachine;

namespace SpinRallyBot.Events.UpdateReceivedConsumers;

public class PublishPipelineCallbackReceivedUpdateReceivedConsumer : IMediatorConsumer<UpdateReceived> {
    private readonly ITelegramBotClient _botClient;
    private readonly IHostEnvironment _hostEnvironment;
    private readonly ILogger<PublishPipelineCallbackReceivedUpdateReceivedConsumer> _logger;
    private readonly IScopedMediator _mediator;

    public PublishPipelineCallbackReceivedUpdateReceivedConsumer(IScopedMediator mediator,
        ILogger<PublishPipelineCallbackReceivedUpdateReceivedConsumer> logger, IHostEnvironment hostEnvironment,
        ITelegramBotClient botClient) {
        _mediator = mediator;
        _logger = logger;
        _hostEnvironment = hostEnvironment;
        _botClient = botClient;
    }

    public async Task Consume(ConsumeContext<UpdateReceived> context) {
        var update = context.Message.Update;
        var cancellationToken = context.CancellationToken;

        if (update is not {
                Message : {
                    Text: { } messageText,
                    MessageId: var messageId,
                    From.Id: var userId,
                    Chat: {
                        Type: var chatType,
                        Id: var chatId
                    }
                } message
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

        if (!result.Is<PipelineData>(out var response) || response is not {
                Message: {
                    Pipeline: var pipeline,
                    Args: var args
                }
            }) {
            throw new UnreachableException();
        }

        args = args is null
            ? new[] { messageText }
            : args.Append(messageText).ToArray();

        var data = string.Join(' ', args);

        try {
            await _mediator.Publish(new CallbackReceived(
                MessageId: null,
                Data: new CallbackData.PipelineData(pipeline, data),
                ChatId: chatId,
                ChatType: chatType,
                UserId: userId
            ), cancellationToken);
        } finally {
            await _mediator.Send(new RemovePipelineState(userId, chatId), cancellationToken);
        }
    }
}