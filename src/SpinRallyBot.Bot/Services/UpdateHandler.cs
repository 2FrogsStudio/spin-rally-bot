using Microsoft.Extensions.Configuration;
using Quartz.Util;
using Telegram.Bot.Exceptions;

namespace SpinRallyBot.Services;

internal class UpdateHandler : IUpdateHandler {
    private readonly IConfiguration _configuration;
    private readonly ILogger<UpdateHandler> _logger;
    private readonly IServiceProvider _serviceProvider;

    public UpdateHandler(ILogger<UpdateHandler> logger, IServiceProvider serviceProvider,
        IConfiguration configuration) {
        _logger = logger;
        _serviceProvider = serviceProvider;
        _configuration = configuration;
    }

    public async Task HandleUpdateAsync(ITelegramBotClient _, Update update,
        CancellationToken cancellationToken) {
        using var updateIdScope = _logger.BeginScope(new Dictionary<string, object> {
            { "UpdateId", update.Id.ToString() },
            { "UpdateType", update.Type.ToString() }
        });

        _logger.LogDebug("Update received: {@Update}", update);

        await using var scope = _serviceProvider.CreateAsyncScope();
        var publishEndpoint = scope.ServiceProvider.GetRequiredService<IPublishEndpoint>();

        try {
            await publishEndpoint.Publish(new UpdateReceived(update, IsBotAdmin(update)), cancellationToken);
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

    private bool IsBotAdmin(Update update) {
        var userId = update.Message?.From?.Id ?? update.CallbackQuery?.From.Id;

        if (userId is null) {
            return false;
        }

        var config = _configuration.GetValue<string>("Bot:AdminIds");
        if (config is null || config.IsNullOrWhiteSpace()) {
            return false;
        }

        return config.Split(',', ';')
            .Select(long.Parse)
            .Contains(userId.Value);
    }
}