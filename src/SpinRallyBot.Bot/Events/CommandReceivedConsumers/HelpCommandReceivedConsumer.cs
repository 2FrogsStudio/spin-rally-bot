using SpinRallyBot.Events.CommandReceivedConsumers.Base;
using SpinRallyBot.Utils;

namespace SpinRallyBot.Events.CommandReceivedConsumers;

public class HelpCommandReceivedConsumer : CommandReceivedConsumerBase {
    public HelpCommandReceivedConsumer(ITelegramBotClient botClient, IMemoryCache memoryCache, IScopedMediator mediator) :
        base(Command.Help, botClient, memoryCache, mediator) { }

    protected override Task ConsumeAndGetReply(long userId, long chatId, string[] args,
        CancellationToken cancellationToken) {
        var text = "Usage:\n" +
                   string.Join('\n', CommandHelpers.CommandAttributeByCommand
                       .Where(c => c.Value is not null)
                       .Select(c => c.Value!)
                       .Select(a => $"{a.Text} - {a.Description}"));

        Text = text.ToEscapedMarkdownV2();
        return Task.CompletedTask;;
    }
}