namespace SpinRallyBot.Events.PullingServiceActivatedConsumers;

public class ShutdownApplicationPullingServiceActivatedConsumer : IConsumer<PullingServiceActivated> {
    private readonly IHostApplicationLifetime _applicationLifetime;
    private readonly ILogger<ShutdownApplicationPullingServiceActivatedConsumer> _logger;

    public ShutdownApplicationPullingServiceActivatedConsumer(
        ILogger<ShutdownApplicationPullingServiceActivatedConsumer> logger,
        IHostApplicationLifetime applicationLifetime) {
        _logger = logger;
        _applicationLifetime = applicationLifetime;
    }

    public Task Consume(ConsumeContext<PullingServiceActivated> context) {
        if (Constants.ApplicationStartDate >= context.Message.ApplicationStartDate) {
            return Task.CompletedTask;
        }

        _logger.LogInformation("There is a new instance of application was started, so shutdown this one");
        _applicationLifetime.StopApplication();
        return Task.CompletedTask;
    }
}
