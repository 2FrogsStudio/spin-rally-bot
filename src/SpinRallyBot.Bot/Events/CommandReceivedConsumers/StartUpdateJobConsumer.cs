using SpinRallyBot.Jobs;

namespace SpinRallyBot.Events.CommandReceivedConsumers;

public class StartUpdateJobConsumer : CommandReceivedConsumerBase {
    private readonly ITelegramBotClient _botClient;
    private readonly IBus _bus;

    public StartUpdateJobConsumer(ITelegramBotClient botClient, IScopedMediator mediator, IBus bus) : base(
        Command.Update, botClient, mediator) {
        _botClient = botClient;
        _bus = bus;
    }

    protected override async Task ConsumeAndGetReply(long userId, long chatId, int? replyToMessageId, string[] args,
        bool isBotAdmin,
        CancellationToken cancellationToken) {
        string formatter = DefaultEndpointNameFormatter.Instance.Consumer<UpdatePlayersJobConsumer>();
        var endpoint = new Uri($"queue:{formatter}");
        ISendEndpoint sendEndpoint = await _bus.GetSendEndpoint(endpoint);
        Message message = await _botClient.SendMessage(chatId, "⏳Обновление запущено..".ToEscapedMarkdownV2(),
            parseMode: ParseMode.MarkdownV2,
            replyParameters: replyToMessageId.HasValue
                ? new ReplyParameters { MessageId = replyToMessageId.Value }
                : null,
            cancellationToken: cancellationToken);
        await sendEndpoint.Send<UpdatePlayersJob>(new {
            __Header_ResponseChatId = chatId,
            __Header_UpdateMessageId = message.MessageId
        }, cancellationToken);
    }
}
