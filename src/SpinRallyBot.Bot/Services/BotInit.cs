namespace SpinRallyBot.Services;

internal class BotInit : IHostedService {
    private readonly ITelegramBotClient _botClient;
    private readonly ILogger<BotInit> _logger;
    private readonly IScopedMediator _mediator;

    public BotInit(ITelegramBotClient botClient,
        ILogger<BotInit> logger,
        IScopedMediator mediator) {
        _botClient = botClient;
        _logger = logger;
        _mediator = mediator;
    }

    public async Task StartAsync(CancellationToken cancellationToken) {
        _logger.LogInformation("Initialize bot (commands, etc)");
        await InitCommands(cancellationToken);
        await _mediator.Send(new InitUpdaterJob(false), cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken) {
        return Task.CompletedTask;
    }

    private async Task InitCommands(CancellationToken cancellationToken) {
        IEnumerable<BotCommand> commands = CommandHelpers.CommandAttributeByCommand.Values
            .Where(d => d is { IsInitCommand: true })
            .Select(d => new BotCommand {
                Command = d?.Text ?? throw new ArgumentNullException(nameof(d)),
                Description = d.Description ?? string.Empty
            });
        await _botClient.SetMyCommands(commands, cancellationToken: cancellationToken);
    }
}
