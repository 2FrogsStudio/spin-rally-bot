namespace SpinRallyBot.Events.UpdateReceivedConsumers;

public class PublishCommandReceivedUpdateReceivedConsumer : IConsumer<UpdateReceived> {
    private readonly ITelegramBotClient _botClient;
    private readonly IHostEnvironment _hostEnvironment;
    private readonly ILogger<PublishCommandReceivedUpdateReceivedConsumer> _logger;
    private readonly IScopedMediator _mediator;

    public PublishCommandReceivedUpdateReceivedConsumer(IScopedMediator mediator,
        ILogger<PublishCommandReceivedUpdateReceivedConsumer> logger,
        IHostEnvironment hostEnvironment,
        ITelegramBotClient botClient) {
        _mediator = mediator;
        _logger = logger;
        _hostEnvironment = hostEnvironment;
        _botClient = botClient;
    }

    public async Task Consume(ConsumeContext<UpdateReceived> context) {
        Update update = context.Message.Update;
        CancellationToken cancellationToken = context.CancellationToken;

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

        bool isBotAdmin = context.Message.IsBotAdmin;
        string[] commandAndArgs = messageText.Split(' ');
        string[] commandAndUserName = commandAndArgs[0].Split('@', 2);
        switch (commandAndUserName.Length) {
            case 1 when update.Message.Chat.Type is not ChatType.Private && _hostEnvironment.IsDevelopment():
                return;
            case 2: {
                BotInfo botInfo = (await _mediator
                    .CreateRequestClient<GetBotInfo>()
                    .GetResponse<BotInfo>(new GetBotInfo(), cancellationToken)).Message;
                if (commandAndUserName[1] != botInfo.Username) {
                    _logger.LogDebug(
                        "Command ignored die to wrong bot username Expected: {ExpectedUserName} Actual: {ActualUserName}",
                        botInfo.Username, commandAndUserName[1]);
                    return;
                }

                break;
            }
        }

        Command command = CommandHelpers.CommandByText.GetValueOrDefault(commandAndUserName[0], Command.Unknown);
        string[] args = commandAndArgs.Length >= 2 ? commandAndArgs[1..] : Array.Empty<string>();

        using IDisposable? commandScope = _logger.BeginScope(new Dictionary<string, object> {
            { "Command", command.ToString() },
            { "Args", string.Join(",", args) }
        });

        if (args is ["help", ..] or [.., "help"]) {
            string? help = CommandHelpers.HelpByCommand[command];
            if (help is not null) {
                await _botClient.SendMessage(
                    chatId,
                    help,
                    parseMode: ParseMode.MarkdownV2,
                    disableNotification: true,
                    replyParameters: new ReplyParameters { MessageId = messageId },
                    replyMarkup: new ReplyKeyboardRemove(),
                    cancellationToken: context.CancellationToken);
                return;
            }
        }

        if (command.IsAdminCommand() && !isBotAdmin) {
            _logger.LogInformation("Called admin command by non-admin user");
            return;
        }

        await _mediator.Publish(new CommandReceived(
            command,
            args,
            chatId,
            chatType,
            messageId,
            null,
            userId,
            isBotAdmin
        ), cancellationToken);
    }
}
