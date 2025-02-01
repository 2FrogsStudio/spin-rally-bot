namespace SpinRallyBot.Attributes;

[AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
public class CommandArgAttribute(string name, string description) : Attribute {
    public string Name { get; } = name;
    public string Description { get; } = description;
    public string[] DependsOn { get; init; } = [];
}
