namespace SpinRallyBot.Attributes;

[AttributeUsage(AttributeTargets.Field)]
public class CommandAttribute : Attribute {
    public CommandAttribute(string text) {
        Text = text;
    }

    public string Text { get; }
    public string? Description { get; init; }
    public bool IsInitCommand { get; init; } = true;
    public bool IsInlineCommand { get; init; } = true;
}