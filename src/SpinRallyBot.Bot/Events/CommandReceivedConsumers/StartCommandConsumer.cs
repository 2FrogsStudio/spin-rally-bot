using SpinRallyBot.Models;
using SpinRallyBot.Subscriptions;
using SpinRallyBot.Utils;

namespace SpinRallyBot.Events.CommandReceivedConsumers;

public class StartCommandConsumer : IMediatorConsumer<CommandReceived> {
    private readonly ITelegramBotClient _botClient;
    private readonly IScopedMediator _mediator;

    public StartCommandConsumer(ITelegramBotClient botClient, IScopedMediator mediator) {
        _botClient = botClient;
        _mediator = mediator;
    }

    public async Task Consume(ConsumeContext<CommandReceived> context) {
        if (context.Message is not {
                Command: Command.Start,
                ChatId: var chatId,
                ChatType: ChatType.Private
            }) {
            return;
        }

        var cancellationToken = context.CancellationToken;

        var commandMenu = CommandHelpers.CommandAttributeByCommand
            .Where(pair => pair.Value?.InlineName != null)
            .Select(pair => {
                var name = pair.Value!.InlineName!;
                var data = new CallbackData.PipelineData(pair.Value.Pipeline);
                return new InlineKeyboardButton(name) {
                    CallbackData = JsonSerializer.Serialize(data)
                };
            })
            .ToArray();

        var subscriptions = (await _mediator.CreateRequestClient<GetSubscriptions>()
            .GetResponse<Subscription[]>(new GetSubscriptions(chatId), cancellationToken)).Message;

        var playerButtons = subscriptions.Select(s => new InlineKeyboardButton(s.Fio) {
            CallbackData = JsonSerializer.Serialize(new CallbackData.CommandData(Command.Find, s.PlayerUrl))
        }).Split(1);
        var commandRows = commandMenu.Split(3).ToArray();

        await _botClient.SendTextMessageAsync(chatId,
            "Главное меню",
            replyMarkup: new InlineKeyboardMarkup(playerButtons.Union(commandRows)),
            cancellationToken: cancellationToken);
    }
}