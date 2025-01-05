namespace SpinRallyBot.Events.CommandReceivedConsumers;

public class HelpCommandReceivedConsumer : CommandReceivedConsumerBase {
    public HelpCommandReceivedConsumer(ITelegramBotClient botClient,
        IScopedMediator mediator) : base(Command.Help, botClient, mediator) { }

    protected override Task ConsumeAndGetReply(long userId, long chatId, int? replyToMessageId, string[] args,
        bool isBotAdmin, CancellationToken cancellationToken) {
        string text = "Usage:\n" +
                      string.Join('\n', CommandHelpers.CommandAttributeByCommand
                          .Where(c => c.Value is not null && (!c.Value.IsAdminCommand || isBotAdmin))
                          .Select(c => c.Value!)
                          .Select(a => $"{a.Text} - {a.Description}"));
        Text = text.ToEscapedMarkdownV2();
        return Task.CompletedTask;
    }
}
