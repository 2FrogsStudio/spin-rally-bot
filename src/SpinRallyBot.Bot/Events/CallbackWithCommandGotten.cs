namespace SpinRallyBot.Events;

public record CallbackWithCommandGotten {
    public CallbackQuery CallbackQuery { get; init; } = null!;
}