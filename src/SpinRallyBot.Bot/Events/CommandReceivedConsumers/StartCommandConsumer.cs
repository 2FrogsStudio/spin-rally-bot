namespace SpinRallyBot.Events.CommandReceivedConsumers;

public class StartCommandConsumer : CommandReceivedConsumerBase {
    private readonly IScopedMediator _mediator;

    public StartCommandConsumer(ITelegramBotClient botClient, IScopedMediator mediator) : base(Command.Start, botClient,
        mediator) {
        _mediator = mediator;
    }

    protected override async Task ConsumeAndGetReply(long userId, long chatId, int? replyToMessageId, string[] args,
        bool isBotAdmin,
        CancellationToken cancellationToken) {
        IEnumerable<InlineKeyboardButton[]> commandMenuRows = CommandHelpers.CommandAttributeByCommand
            .Where(pair => pair.Value?.InlineName != null)
            .Select(pair => {
                string name = pair.Value!.InlineName!;
                var data = new NavigationData.PipelineData(pair.Value.Pipeline);
                return new InlineKeyboardButton(name) {
                    CallbackData = JsonSerializer.Serialize(data)
                };
            })
            .Split(3);

        GetSubscriptionsByChatIdResult subscriptions = (await _mediator
                .CreateRequestClient<GetSubscriptionsByChatId>()
                .GetResponse<GetSubscriptionsByChatIdResult>(new GetSubscriptionsByChatId(chatId), cancellationToken))
            .Message;

        InlineKeyboardButton[][] playerButtonRows = subscriptions.Subscriptions
            .Select(s => new InlineKeyboardButton($"{s.Fio} ({s.Rating})") {
                CallbackData = JsonSerializer.Serialize(new NavigationData.CommandData(Command.Find, s.PlayerUrl))
            }).Split(1).ToArray();

        Text = "Главное меню";
        InlineKeyboard = playerButtonRows.Union(commandMenuRows);
        await _mediator.Send(new ResetBackNavigation(userId, chatId), cancellationToken);
    }
}
