using SpinRallyBot.Events.BotCommandReceivedConsumers.Base;
using SpinRallyBot.Utils;
using Telegram.Bot.Extensions.Markup;

namespace SpinRallyBot.Events.BotCommandReceivedConsumers;

public class HelpBotCommandReceivedConsumer : BotCommandReceivedConsumerBase {
    public HelpBotCommandReceivedConsumer(ITelegramBotClient botClient, IMemoryCache memoryCache) :
        base(Command.Help, botClient, memoryCache) { }

    protected override Task<string?> ConsumeAndGetReply(string[] args, Message message, long chatId, int messageThreadId,
        bool isAdmin,
        CancellationToken cancellationToken) {
        var text = "Usage:\n" +
                   string.Join('\n', CommandHelpers.CommandAttributeByCommand
                       .Where(c => c.Value is not null)
                       .Select(c => c.Value!)
                       .Select(a => $"{a.Text} - {a.Description}"));

        return Task.FromResult((string?)Tools.EscapeMarkdown(text, ParseMode.MarkdownV2));
    }
}
