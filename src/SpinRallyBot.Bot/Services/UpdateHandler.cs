using Telegram.Bot.Exceptions;

namespace SpinRallyBot.Services;

internal class UpdateHandler : IUpdateHandler {
    private readonly ILogger<UpdateHandler> _logger;
    private readonly IServiceProvider _serviceProvider;

    public UpdateHandler(ILogger<UpdateHandler> logger, IServiceProvider serviceProvider) {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    public async Task HandleUpdateAsync(ITelegramBotClient _, Update update,
        CancellationToken cancellationToken) {
        using var updateIdScope = _logger.BeginScope(new Dictionary<string, object> {
            { "UpdateId", update.Id.ToString() },
            { "UpdateType", update.Type.ToString() }
        });

        _logger.LogDebug("Update received: {@Update}", update);

        await using var scope = _serviceProvider.CreateAsyncScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IScopedMediator>();
        try {
            await mediator.Publish(new UpdateReceived(update), cancellationToken);
        } catch (Exception ex) {
            _logger.LogError(ex, "UpdateReceived failed: {@Update}", update);
        }
    }

    public async Task HandlePollingErrorAsync(ITelegramBotClient _, Exception exception,
        CancellationToken cancellationToken) {
        _logger.LogError(exception, "Telegram API Error");
        if (exception is ApiRequestException) {
            await Task.Delay(TimeSpan.FromSeconds(2), cancellationToken);
        }
    }
}