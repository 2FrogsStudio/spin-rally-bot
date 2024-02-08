using Quartz;

namespace SpinRallyBot.Services;

internal class BotInit : IHostedService {
    private readonly ITelegramBotClient _botClient;
    private readonly IBus _bus;
    private readonly IHostEnvironment _hostEnvironment;
    private readonly ILogger<BotInit> _logger;
    private readonly ISchedulerFactory _schedulerFactory;
    private readonly IScopedMediator _mediator;

    public BotInit(ITelegramBotClient botClient,
                   ILogger<BotInit> logger,
                   IBus bus,
                   ISchedulerFactory schedulerFactory,
                   IHostEnvironment hostEnvironment,
                   IScopedMediator mediator) {
        _botClient = botClient;
        _logger = logger;
        _bus = bus;
        _schedulerFactory = schedulerFactory;
        _hostEnvironment = hostEnvironment;
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
        var commands = CommandHelpers.CommandAttributeByCommand.Values
                                     .Where(d => d is { IsInitCommand: true })
                                     .Select(d => new BotCommand {
                                         Command = d!.Text!,
                                         Description = d.Description ?? string.Empty
                                     });
        await _botClient.SetMyCommandsAsync(commands, cancellationToken: cancellationToken);
    }
}
