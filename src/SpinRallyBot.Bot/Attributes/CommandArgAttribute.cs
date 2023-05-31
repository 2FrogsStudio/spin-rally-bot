namespace SpinRallyBot.Attributes;

[AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
public class CommandArgAttribute : Attribute {
    public CommandArgAttribute(string name, string description) {
        Name = name;
        Description = description;
    }
    public string Name { get; }
    public string Description { get; }
    public string[] DependsOn { get; init; } = Array.Empty<string>();
}