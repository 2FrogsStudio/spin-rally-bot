namespace SpinRallyBot.Services;

internal class BotInit : IHostedService {
    private readonly ITelegramBotClient _botClient;
    private readonly ILogger<BotInit> _logger;
    private readonly IServiceProvider _serviceProvider;

    public BotInit(ITelegramBotClient botClient, ILogger<BotInit> logger, IServiceProvider serviceProvider) {
        _botClient = botClient;
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    public async Task StartAsync(CancellationToken cancellationToken) {
        _logger.LogInformation("Initialize bot (commands, etc)");
        await InitCommands(cancellationToken);
        await using AsyncServiceScope scope = _serviceProvider.CreateAsyncScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IScopedMediator>();
        await mediator.Send(new InitUpdaterJob(false), cancellationToken);
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
