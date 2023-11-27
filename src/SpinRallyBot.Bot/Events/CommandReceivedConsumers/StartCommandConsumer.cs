namespace SpinRallyBot.Events.CommandReceivedConsumers;

public class StartCommandConsumer(ITelegramBotClient botClient, IScopedMediator mediator)
    : CommandReceivedConsumerBase(Command.Start, botClient, mediator) {
    private readonly IScopedMediator _mediator1 = mediator;

    protected override async Task ConsumeAndGetReply(long userId, long chatId, int? replyToMessageId, string[] args,
        bool isBotAdmin,
        CancellationToken cancellationToken) {
        var commandMenuRows = CommandHelpers.CommandAttributeByCommand
            .Where(pair => pair.Value?.InlineName != null)
            .Select(pair => {
                var name = pair.Value!.InlineName!;
                var data = new NavigationData.PipelineData(pair.Value.Pipeline);
                return new InlineKeyboardButton(name) {
                    CallbackData = JsonSerializer.Serialize(data)
                };
            })
            .Split(3);

        var subscriptions = (await _mediator1
                .CreateRequestClient<GetSubscriptionsByChatId>()
                .GetResponse<GetSubscriptionsByChatIdResult>(new GetSubscriptionsByChatId(chatId), cancellationToken))
            .Message;

        var playerButtonRows = subscriptions.Subscriptions
            .Select(s => new InlineKeyboardButton($"{s.Fio} ({s.Rating})") {
                CallbackData = JsonSerializer.Serialize(new NavigationData.CommandData(Command.Find, s.PlayerUrl))
            }).Split(1).ToArray();

        Text = "Главное меню";
        InlineKeyboard = playerButtonRows.Union(commandMenuRows);
        await _mediator1.Send(new ResetBackNavigation(userId, chatId), cancellationToken);
    }
}