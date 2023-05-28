using System.Diagnostics;
using SpinRallyBot.Queries.GetCommandPipelineState;
using SpinRallyBot.Utils;

namespace SpinRallyBot.Events.UpdateReceivedConsumers;

public class CommandUpdateReceivedConsumer : IConsumer<UpdateReceived>, IMediatorConsumer {
    private readonly ITelegramBotClient _botClient;
    private readonly IMemoryCache _cache;
    private readonly IHostEnvironment _hostEnvironment;
    private readonly ILogger<CommandUpdateReceivedConsumer> _logger;
    private readonly IScopedMediator _mediator;

    public CommandUpdateReceivedConsumer(IScopedMediator mediator,
        ILogger<CommandUpdateReceivedConsumer> logger, IHostEnvironment hostEnvironment,
        ITelegramBotClient botClient, IMemoryCache cache) {
        _mediator = mediator;
        _logger = logger;
        _hostEnvironment = hostEnvironment;
        _botClient = botClient;
        _cache = cache;
    }

    public async Task Consume(ConsumeContext<UpdateReceived> context) {
        var update = context.Message.Update;
        var cancellationToken = context.CancellationToken;

        if (update is not {
                Message : {
                    Text: { } messageText,
                    MessageId: var messageId,
                    Chat.Id: var chatId
                }
            }) {
            return;
        }

        Command command;
        string[] args;
        if (messageText.StartsWith('/')) {
            var commandAndArgs = messageText.Split(' ');
            var commandAndUserName = commandAndArgs[0].Split('@', 2);
            switch (commandAndUserName.Length) {
                case 1 when update.Message.Chat.Type is not ChatType.Private && _hostEnvironment.IsDevelopment():
                    return;
                case 2: {
                    var botUsername = await GetBotUsername(cancellationToken);
                    if (commandAndUserName[1] != botUsername) {
                        _logger.LogDebug("Command ignored die to wrong bot username Expected: {ExpectedUserName} Actual: {ActualUserName}",
                            botUsername, commandAndUserName[1]);
                        return;
                    }

                    break;
                }
            }

            command = CommandHelpers.CommandByText.TryGetValue(commandAndUserName[0], out var cmd)
                ? cmd
                : Command.Unknown;
            args = commandAndArgs.Length >= 2 ? commandAndArgs[1..] : Array.Empty<string>();
        } else {
            var response = await _mediator
                .CreateRequestClient<GetPipelineStateQuery>()
                .GetResponse<PipelineStateResult, NoPipelineStateResult>(new { }, cancellationToken);
            if (response.Is(out Response<NoPipelineStateResult>? _)) {
                await _botClient.SendTextMessageAsync(chatId, "Not in pipeline context",
                    cancellationToken: cancellationToken);
                return;
            }

            if (response.Is(out Response<PipelineStateResult>? pipelineState)) {
                command = pipelineState.Message.Command;
                args = messageText.Split(' ');
            } else {
                throw new UnreachableException();
            }
        }

        using var commandScope = _logger.BeginScope(new Dictionary<string, string> {
            { "Command", command.ToString() },
            { "Arg", string.Join(' ', args) }
        });

        if (args.Length == 1 && args[0].Equals("help", StringComparison.InvariantCultureIgnoreCase)) {
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

        await _mediator.Publish(new BotCommandReceived(
            command,
            args,
            update.Message
        ), cancellationToken);
    }

    private async Task<string?> GetBotUsername(CancellationToken cancellationToken) {
        return await _cache.GetOrCreateAsync("BotUsername", async entry => {
            entry.Size = 1;
            entry.Priority = CacheItemPriority.NeverRemove;
            return (await _botClient.GetMeAsync(cancellationToken)).Username;
        });
    }
}