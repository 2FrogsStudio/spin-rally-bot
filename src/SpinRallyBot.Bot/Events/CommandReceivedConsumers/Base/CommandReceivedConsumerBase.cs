namespace SpinRallyBot.Events.CommandReceivedConsumers.Base;

public abstract class CommandReceivedConsumerBase : IMediatorConsumer<CommandReceived> {
    private readonly ITelegramBotClient _bot;
    private readonly Command _command;
    private readonly IMemoryCache _memoryCache;

    protected CommandReceivedConsumerBase(Command command, ITelegramBotClient bot, IMemoryCache memoryCache) {
        _command = command;
        _bot = bot;
        _memoryCache = memoryCache;
    }

    public async Task Consume(ConsumeContext<CommandReceived> context) {
        if (context.Message.Command != _command) {
            return;
        }

        var cancellationToken = context.CancellationToken;

        if (context.Message is not {
                Command: var command,
                Args: var args,
                ChatId: var chatId,
                ChatType: var chatType,
                MessageId: var messageId,
                UserId: var userId
            }) {
            return;
        }

        var isAdmin = chatType == ChatType.Private || await IsChatAdmin(chatId, userId, cancellationToken);

        // ReSharper disable once NullCoalescingConditionIsAlwaysNotNullAccordingToAPIContract
        var replyText = await ConsumeAndGetReply(chatId, args, cancellationToken);

        if (replyText is null) {
            return;
        }

        await _bot.SendTextMessageAsync(
            chatId,
            replyText,
            parseMode: ParseMode.MarkdownV2,
            disableWebPagePreview: true,
            disableNotification: true,
            replyToMessageId: chatType is ChatType.Private ? null : messageId,
            cancellationToken: cancellationToken
        );
    }

    private async Task<bool> IsChatAdmin(long chatId, long userId, CancellationToken cancellationToken) {
        var cache = await _memoryCache.GetOrCreateAsync($"AdminIdsByChatId_{chatId}", async entry => {
            entry.SetAbsoluteExpiration(TimeSpan.FromMinutes(1));
            entry.SetSize(1);
            var admins = await _bot.GetChatAdministratorsAsync(chatId, cancellationToken);
            return admins.Select(a => a.User.Id).ToArray();
        });

        return cache?.Contains(userId) ?? false;
    }

    protected abstract Task<string?> ConsumeAndGetReply(long chatId, string[] args, CancellationToken cancellationToken);
}