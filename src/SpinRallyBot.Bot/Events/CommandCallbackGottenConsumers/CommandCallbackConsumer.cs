using SpinRallyBot.Attributes;
using SpinRallyBot.Commands.SetCommandPipelineState;

namespace SpinRallyBot.Events.CommandCallbackGottenConsumers;

public class CommandCallbackConsumer : IConsumer<CallbackWithCommandGotten>, IMediatorConsumer {
    private readonly ITelegramBotClient _botClient;
    private readonly IScopedMediator _mediator;

    public CommandCallbackConsumer(ITelegramBotClient botClient, IScopedMediator mediator) {
        _botClient = botClient;
        _mediator = mediator;
    }

    public async Task Consume(ConsumeContext<CallbackWithCommandGotten> context) {
        var callback = context.Message.CallbackQuery;
        if (callback is not {
                Data: var data,
                Message.Chat.Id: var chatId
            } || !Enum.TryParse(data, out Command command)) {
            return;
        }

        await _mediator.Send<SetPipelineStateCommand>(new {
            Command = command
        }, context.CancellationToken);

        var args = command.GetAttributesOfType<CommandArgAttribute>();
        if (args.Length > 0) {
            await _botClient.SendTextMessageAsync(chatId,
                "Ok. Send me the args for command:",
                replyMarkup: new ReplyKeyboardRemove(),
                cancellationToken: context.CancellationToken);
            return;
        }

        await _mediator.Publish(new BotCommandReceived(
            command,
            Array.Empty<string>(),
            context.Message.CallbackQuery.Message!
        ), context.CancellationToken);
    }
}