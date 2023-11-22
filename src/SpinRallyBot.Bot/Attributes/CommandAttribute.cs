namespace SpinRallyBot.Attributes;

[AttributeUsage(AttributeTargets.Field)]
public class CommandAttribute : Attribute {
    public string? Text { get; init; }
    public string? InlineName { get; init; }
    public string? Description { get; init; }
    public bool IsInitCommand { get; init; } = true;
    public Pipeline Pipeline { get; init; }
    public bool IsAdminCommand { get; init; }
}