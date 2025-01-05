namespace SpinRallyBot.Events.CommandReceivedConsumers;

public class RescheduleJobConsumer(ITelegramBotClient botClient, IScopedMediator mediator)
    : CommandReceivedConsumerBase(Command.RescheduleJob, botClient, mediator) {
    private readonly IScopedMediator _mediator = mediator;

    protected override async Task ConsumeAndGetReply(long userId,
        long chatId,
        int? replyToMessageId,
        string[] args,
        bool isBotAdmin,
        CancellationToken cancellationToken) {
        await _mediator.Send(new InitUpdaterJob(true), cancellationToken);
    }
}
