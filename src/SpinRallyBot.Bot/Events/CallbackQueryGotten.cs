namespace SpinRallyBot.Events;

public record CallbackReceived(CallbackData Data, int? MessageId, long ChatId, ChatType ChatType, long UserId);