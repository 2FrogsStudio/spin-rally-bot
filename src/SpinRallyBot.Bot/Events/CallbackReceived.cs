namespace SpinRallyBot.Events;

public record CallbackReceived(NavigationData Data, int? MessageId, long ChatId, ChatType ChatType, long UserId);