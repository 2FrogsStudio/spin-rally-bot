namespace SpinRallyBot.Events.CommandReceivedConsumers;

public class HelpCommandReceivedConsumer(
    ITelegramBotClient botClient,
    IScopedMediator mediator) : CommandReceivedConsumerBase(Command.Help, botClient, mediator) {
    protected override Task ConsumeAndGetReply(long userId, long chatId, int? replyToMessageId, string[] args,
        bool isBotAdmin, CancellationToken cancellationToken) {
        string text = "Usage:\n" +
                      string.Join('\n', CommandHelpers.CommandAttributeByCommand
                          .Select(c => c.Value)
                          .Where(c => c is not null && (!c.IsAdminCommand || isBotAdmin))
                          .Select(a => $"{a?.Text} - {a?.Description}"));
        Text = text.ToEscapedMarkdownV2();
        return Task.CompletedTask;
    }
}
