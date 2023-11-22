using SpinRallyBot.Jobs;

namespace SpinRallyBot.Events.CommandReceivedConsumers;

public class StartUpdateJobConsumer(ITelegramBotClient botClient, IScopedMediator mediator, IBus bus)
    : CommandReceivedConsumerBase(Command.Update, botClient, mediator) {
    private readonly ITelegramBotClient _botClient = botClient;

    protected override async Task ConsumeAndGetReply(long userId, long chatId, int? replyToMessageId, string[] args,
        bool isBotAdmin,
        CancellationToken cancellationToken) {
        var formatter = DefaultEndpointNameFormatter.Instance.Consumer<UpdatePlayersJobConsumer>();
        var endpoint = new Uri($"queue:{formatter}");
        var sendEndpoint = await bus.GetSendEndpoint(endpoint);
        var message = await _botClient.SendTextMessageAsync(chatId, "⏳Update started..",
            replyToMessageId: replyToMessageId,
            cancellationToken: cancellationToken);
        await sendEndpoint.Send<UpdatePlayersJob>(new {
            __Header_RespondChatId = chatId,
            __Header_RespondReplyToMessageId = message.MessageId
        }, cancellationToken);
    }
}
