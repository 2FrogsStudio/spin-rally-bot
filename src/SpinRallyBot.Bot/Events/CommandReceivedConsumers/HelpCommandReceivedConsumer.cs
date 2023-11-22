using Microsoft.Extensions.Configuration;

namespace SpinRallyBot.Events.CommandReceivedConsumers;

public class HelpCommandReceivedConsumer(
    ITelegramBotClient botClient,
    IScopedMediator mediator,
    IConfiguration configuration)
    : CommandReceivedConsumerBase(Command.Help, botClient, mediator) {
    private readonly IConfiguration _configuration = configuration;

    protected override Task ConsumeAndGetReply(long userId, long chatId, int? replyToMessageId, string[] args,
        bool isBotAdmin, CancellationToken cancellationToken) {
        var text = "Usage:\n" +
                   string.Join('\n', CommandHelpers.CommandAttributeByCommand
                       .Where(c => c.Value is not null && (!c.Value.IsAdminCommand || isBotAdmin))
                       .Select(c => c.Value!)
                       .Select(a => $"{a.Text} - {a.Description}"));
        Text = text.ToEscapedMarkdownV2();
        return Task.CompletedTask;
    }
}