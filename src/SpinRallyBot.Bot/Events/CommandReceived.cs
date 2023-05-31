namespace SpinRallyBot.Events;

public record CommandReceived(Command Command, string[] Args, long ChatId, ChatType ChatType, int? MessageId, long UserId);