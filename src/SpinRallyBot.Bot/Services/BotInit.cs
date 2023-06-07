using Quartz;
using SpinRallyBot.Jobs;

namespace SpinRallyBot.Services;

internal class BotInit : IHostedService {
    private readonly ITelegramBotClient _botClient;
    private readonly IBus _bus;
    private readonly ILogger<BotInit> _logger;
    private readonly ISchedulerFactory _schedulerFactory;

    public BotInit(ITelegramBotClient botClient, ILogger<BotInit> logger, IBus bus,
        ISchedulerFactory schedulerFactory) {
        _botClient = botClient;
        _logger = logger;
        _bus = bus;
        _schedulerFactory = schedulerFactory;
    }

    public async Task StartAsync(CancellationToken cancellationToken) {
        _logger.LogInformation("Initialize bot (commands, etc)");
        await InitCommands(cancellationToken);
        await InitUpdaterJob(cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken) {
        return Task.CompletedTask;
    }

    private async Task InitUpdaterJob(CancellationToken cancellationToken) {
        var schedule = new UpdatePlayersJobSchedule();
        var currentCron = await GetCurrentCronSchedule(schedule.ScheduleId, schedule.ScheduleGroup, cancellationToken);

        if (currentCron != schedule.CronExpression) {
            var sendEndpoint = await _bus.GetSendEndpoint(new Uri("queue:quartz"));
            var formatter = DefaultEndpointNameFormatter.Instance.Consumer<UpdatePlayersJobConsumer>();
            var endpoint = new Uri($"queue:{formatter}");
            await sendEndpoint.ScheduleRecurringSend(endpoint, schedule, new UpdatePlayersJob(),
                cancellationToken);
        }
    }

    private async Task<string?> GetCurrentCronSchedule(string scheduleId,
        string scheduleGroup, CancellationToken cancellationToken) {
        var scheduler = await _schedulerFactory.GetScheduler(cancellationToken);
        var triggerKey = new TriggerKey("Recurring.Trigger." + scheduleId,
            scheduleGroup);
        var trigger = await scheduler.GetTrigger(triggerKey, cancellationToken);

        return trigger is ICronTrigger {
            CronExpressionString: { } cronExp
        }
            ? cronExp
            : null;
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