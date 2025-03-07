namespace SpinRallyBot.Events.CommandReceivedConsumers.Base;

public abstract class CommandReceivedConsumerBase : IMediatorConsumer<CommandReceived> {
    private readonly ITelegramBotClient _bot;
    private readonly Command _command;
    private readonly IScopedMediator _mediator;

    protected CommandReceivedConsumerBase(Command command, ITelegramBotClient bot,
        IScopedMediator mediator) {
        _command = command;
        _bot = bot;
        _mediator = mediator;
    }

    protected string? Text { get; set; }
    protected IEnumerable<IEnumerable<InlineKeyboardButton>>? InlineKeyboard { get; set; }

    public async Task Consume(ConsumeContext<CommandReceived> context) {
        if (context.Message.Command != _command) {
            return;
        }

        CancellationToken cancellationToken = context.CancellationToken;

        if (context.Message is not {
                Args: var args,
                ChatId: var chatId,
                ChatType: var chatType,
                ReplyToMessageId: var replyToMessageId,
                MenuMessageId: var menuMessageId,
                UserId: var userId,
                IsBotAdmin: var isBotAdmin
            }) {
            return;
        }

        await ConsumeAndGetReply(userId, chatId, replyToMessageId, args, isBotAdmin, cancellationToken);

        if (Text is null) {
            return;
        }

        Response<GetBackNavigationResult[], EmptyNavigation> response = await _mediator
            .CreateRequestClient<GetBackNavigationList>()
            .GetResponse<GetBackNavigationResult[], EmptyNavigation>(new GetBackNavigationList(userId, chatId),
                cancellationToken);

        if (response.Is<GetBackNavigationResult[]>(out Response<GetBackNavigationResult[]>? listResponse)
            && listResponse.Message is { } list) {
            IEnumerable<InlineKeyboardButton[]> backButtons = list.Select(l => new InlineKeyboardButton(l.Name) {
                CallbackData = JsonSerializer.Serialize(new NavigationData.BackData { Guid = l.Guid })
            }).Split(3);

            InlineKeyboard = InlineKeyboard is null ? backButtons : InlineKeyboard.Union(backButtons);
        }

        // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
        if (menuMessageId.HasValue) {
            await _bot.EditMessageText(
                chatId,
                menuMessageId.Value,
                Text,
                ParseMode.MarkdownV2,
                replyMarkup: InlineKeyboard is not null ? new InlineKeyboardMarkup(InlineKeyboard) : null,
                cancellationToken: cancellationToken
            );
        } else {
            await _bot.SendMessage(
                chatId,
                Text,
                parseMode: ParseMode.MarkdownV2,
                disableNotification: true,
                replyMarkup: InlineKeyboard is not null ? new InlineKeyboardMarkup(InlineKeyboard) : null,
                replyParameters: chatType is ChatType.Private || !replyToMessageId.HasValue
                    ? null
                    : new ReplyParameters { MessageId = replyToMessageId.Value },
                cancellationToken: cancellationToken
            );
        }
    }

    protected abstract Task ConsumeAndGetReply(long userId, long chatId, int? replyToMessageId, string[] args,
        bool isBotAdmin,
        CancellationToken cancellationToken);
}
