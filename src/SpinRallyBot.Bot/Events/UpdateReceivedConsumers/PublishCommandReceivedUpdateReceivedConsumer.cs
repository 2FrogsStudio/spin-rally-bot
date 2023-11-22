namespace SpinRallyBot.Events.UpdateReceivedConsumers;

public class PublishCommandReceivedUpdateReceivedConsumer(
    IScopedMediator mediator,
    ILogger<PublishCommandReceivedUpdateReceivedConsumer> logger,
    IHostEnvironment hostEnvironment,
    ITelegramBotClient botClient)
    : IConsumer<UpdateReceived> {
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

        var isBotAdmin = context.Message.IsBotAdmin;
        var commandAndArgs = messageText.Split(' ');
        var commandAndUserName = commandAndArgs[0].Split('@', 2);
        switch (commandAndUserName.Length) {
            case 1 when update.Message.Chat.Type is not ChatType.Private && hostEnvironment.IsDevelopment():
                return;
            case 2: {
                var botInfo = (await mediator
                    .CreateRequestClient<GetBotInfo>()
                    .GetResponse<BotInfo>(new GetBotInfo(), cancellationToken)).Message;
                if (commandAndUserName[1] != botInfo.Username) {
                    logger.LogDebug(
                        "Command ignored die to wrong bot username Expected: {ExpectedUserName} Actual: {ActualUserName}",
                        botInfo.Username, commandAndUserName[1]);
                    return;
                }

                break;
            }
        }

        var command = CommandHelpers.CommandByText.GetValueOrDefault(commandAndUserName[0], Command.Unknown);
        var args = commandAndArgs.Length >= 2 ? commandAndArgs[1..] : Array.Empty<string>();

        using var commandScope = logger.BeginScope(new Dictionary<string, object> {
            { "Command", command.ToString() },
            { "Args", string.Join(",", args) }
        });

        if (args is ["help", ..] or [.., "help"]) {
            var help = CommandHelpers.HelpByCommand[command];
            if (help is not null) {
                await botClient.SendTextMessageAsync(
                    chatId,
                    help,
                    parseMode: ParseMode.MarkdownV2,
                    disableNotification: true,
                    replyToMessageId: messageId,
                    replyMarkup: new ReplyKeyboardRemove(),
                    cancellationToken: context.CancellationToken);
                return;
            }
        }

        if (command.IsAdminCommand() && !isBotAdmin) {
            logger.LogInformation("Called admin command by non-admin user");
            return;
        }

        await mediator.Publish(new CommandReceived(
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
