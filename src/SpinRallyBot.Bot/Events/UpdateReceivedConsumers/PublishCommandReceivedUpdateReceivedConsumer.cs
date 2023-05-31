using SpinRallyBot.Queries;
using SpinRallyBot.Utils;

namespace SpinRallyBot.Events.UpdateReceivedConsumers;

public class PublishCommandReceivedUpdateReceivedConsumer : IMediatorConsumer<UpdateReceived> {
    private readonly ITelegramBotClient _botClient;
    private readonly IHostEnvironment _hostEnvironment;
    private readonly ILogger<PublishCommandReceivedUpdateReceivedConsumer> _logger;
    private readonly IScopedMediator _mediator;

    public PublishCommandReceivedUpdateReceivedConsumer(IScopedMediator mediator,
        ILogger<PublishCommandReceivedUpdateReceivedConsumer> logger, IHostEnvironment hostEnvironment,
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
                }
            }
            || chatType is not ChatType.Private
            || !messageText.StartsWith('/')) {
            return;
        }

        var commandAndArgs = messageText.Split(' ');
        var commandAndUserName = commandAndArgs[0].Split('@', 2);
        switch (commandAndUserName.Length) {
            case 1 when update.Message.Chat.Type is not ChatType.Private && _hostEnvironment.IsDevelopment():
                return;
            case 2: {
                var botInfo = (await _mediator
                    .CreateRequestClient<GetBotInfo>()
                    .GetResponse<BotInfo>(new GetBotInfo(), cancellationToken)).Message;
                if (commandAndUserName[1] != botInfo.Username) {
                    _logger.LogDebug("Command ignored die to wrong bot username Expected: {ExpectedUserName} Actual: {ActualUserName}",
                        botInfo.Username, commandAndUserName[1]);
                    return;
                }

                break;
            }
        }

        var command = CommandHelpers.CommandByText.TryGetValue(commandAndUserName[0], out var cmd)
            ? cmd
            : Command.Unknown;
        var args = commandAndArgs.Length >= 2 ? commandAndArgs[1..] : Array.Empty<string>();

        using var commandScope = _logger.BeginScope(new Dictionary<string, string> {
            { "Command", command.ToString() },
            { "Args", string.Join(' ', args) }
        });

        if (args is ["help", ..] or [.., "help"]) {
            var help = CommandHelpers.HelpByCommand[command];
            if (help is not null) {
                await _botClient.SendTextMessageAsync(
                    chatId,
                    help,
                    parseMode: ParseMode.MarkdownV2,
                    replyToMessageId: messageId,
                    replyMarkup: new ReplyKeyboardRemove(),
                    cancellationToken: context.CancellationToken);
                return;
            }
        }

        await _mediator.Publish(new CommandReceived(
            command,
            args,
            chatId,
            chatType,
            messageId,
            null,
            userId
        ), cancellationToken);
    }
}