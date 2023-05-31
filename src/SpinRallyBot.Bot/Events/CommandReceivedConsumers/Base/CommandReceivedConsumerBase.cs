using SpinRallyBot.BackNavigations;

namespace SpinRallyBot.Events.CommandReceivedConsumers.Base;

public abstract class CommandReceivedConsumerBase : IMediatorConsumer<CommandReceived> {
    private readonly ITelegramBotClient _bot;
    private readonly Command _command;
    private readonly IMemoryCache _memoryCache;
    private readonly IScopedMediator _mediator;

    protected CommandReceivedConsumerBase(Command command, ITelegramBotClient bot, IMemoryCache memoryCache, IScopedMediator mediator) {
        _command = command;
        _bot = bot;
        _memoryCache = memoryCache;
        _mediator = mediator;
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
                ReplyToMessageId: var replyToMessageId,
                MenuMessageId: var menuMessageId,
                UserId: var userId
            }) {
            return;
        }

        var isAdmin = chatType == ChatType.Private || await IsChatAdmin(chatId, userId, cancellationToken);

        // ReSharper disable once NullCoalescingConditionIsAlwaysNotNullAccordingToAPIContract
        await ConsumeAndGetReply(userId, chatId, args, cancellationToken);

        if (Text is null) {
            return;
        }

        var response = await _mediator
            .CreateRequestClient<GetBackNavigationList>()
            .GetResponse<GetBackNavigationResult[], EmptyNavigation>(new GetBackNavigationList(userId, chatId), cancellationToken);

        if (response.Is<GetBackNavigationResult[]>(out var listResponse)
            && listResponse.Message is { } list) {
            var backButtons = list.Select(l => new InlineKeyboardButton(l.Name) {
                CallbackData = JsonSerializer.Serialize(new NavigationData.BackData { Guid = l.Guid })
            }).Split(1);

            InlineKeyboard = InlineKeyboard is null ? backButtons : InlineKeyboard.Union(backButtons);
        }

        if (menuMessageId.HasValue) {
            await _bot.EditMessageTextAsync(
                chatId,
                menuMessageId.Value,
                Text,
                parseMode: ParseMode.MarkdownV2,
                disableWebPagePreview: true,
                replyMarkup: InlineKeyboard is not null ? new InlineKeyboardMarkup(InlineKeyboard) : null,
                cancellationToken: cancellationToken
            );
        } else {
            await _bot.SendTextMessageAsync(
                chatId,
                Text,
                parseMode: ParseMode.MarkdownV2,
                disableWebPagePreview: true,
                disableNotification: true,
                replyMarkup: InlineKeyboard is not null ? new InlineKeyboardMarkup(InlineKeyboard) : null,
                replyToMessageId: chatType is ChatType.Private ? null : replyToMessageId,
                cancellationToken: cancellationToken
            );
        }
    }

    protected string? Text { get; set; }
    protected IEnumerable<IEnumerable<InlineKeyboardButton>>? InlineKeyboard { get; set; }

    private async Task<bool> IsChatAdmin(long chatId, long userId, CancellationToken cancellationToken) {
        var cache = await _memoryCache.GetOrCreateAsync($"AdminIdsByChatId_{chatId}", async entry => {
            entry.SetAbsoluteExpiration(TimeSpan.FromMinutes(1));
            entry.SetSize(1);
            var admins = await _bot.GetChatAdministratorsAsync(chatId, cancellationToken);
            return admins.Select(a => a.User.Id).ToArray();
        });

        return cache?.Contains(userId) ?? false;
    }

    protected abstract Task ConsumeAndGetReply(long userId, long chatId, string[] args, CancellationToken cancellationToken);
}