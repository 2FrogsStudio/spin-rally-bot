namespace SpinRallyBot.Queries;

public record GetBotInfo;

public record BotInfo(string Username);

public class GetBotInfoConsumer : IMediatorConsumer<GetBotInfo> {
    private readonly ITelegramBotClient _botClient;
    private readonly IMemoryCache _memoryCache;

    public GetBotInfoConsumer(IMemoryCache memoryCache, ITelegramBotClient botClient) {
        _memoryCache = memoryCache;
        _botClient = botClient;
    }

    public async Task Consume(ConsumeContext<GetBotInfo> context) {
        CancellationToken cancellationToken = context.CancellationToken;
        BotInfo? botInfo = await _memoryCache.GetOrCreateAsync("BotInfo", async entry => {
            entry.Size = 1;
            entry.SetPriority(CacheItemPriority.NeverRemove);
            User botUser = await _botClient.GetMe(cancellationToken);
            return new BotInfo(botUser.Username ?? throw new InvalidOperationException());
        });
        await context.RespondAsync(botInfo ?? throw new InvalidOperationException());
    }
}
