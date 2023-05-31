namespace SpinRallyBot.Events;

public record CallbackReceived(NavigationData NavigationData, int? MessageId, long ChatId, ChatType ChatType, long UserId);