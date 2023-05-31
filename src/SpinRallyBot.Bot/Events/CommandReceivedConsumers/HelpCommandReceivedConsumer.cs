using SpinRallyBot.Events.CommandReceivedConsumers.Base;
using SpinRallyBot.Utils;
using Telegram.Bot.Extensions.Markup;

namespace SpinRallyBot.Events.CommandReceivedConsumers;

public class HelpCommandReceivedConsumer : CommandReceivedConsumerBase {
    public HelpCommandReceivedConsumer(ITelegramBotClient botClient, IMemoryCache memoryCache) :
        base(Command.Help, botClient, memoryCache) { }

    protected override Task<string?> ConsumeAndGetReply(long chatId, string[] args,
        CancellationToken cancellationToken) {
        var text = "Usage:\n" +
                   string.Join('\n', CommandHelpers.CommandAttributeByCommand
                       .Where(c => c.Value is not null)
                       .Select(c => c.Value!)
                       .Select(a => $"{a.Text} - {a.Description}"));

        return Task.FromResult((string?)Tools.EscapeMarkdown(text, ParseMode.MarkdownV2));
    }
}