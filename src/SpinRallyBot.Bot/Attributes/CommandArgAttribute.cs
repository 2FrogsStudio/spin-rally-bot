namespace SpinRallyBot.Attributes;

[AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
public class CommandArgAttribute : Attribute {
    public CommandArgAttribute(string name, string description, CommandArgType type, params string[] dependsOn) {
        Name = name;
        Description = description;
        Type = type;
        DependsOn = dependsOn;
    }

    public string Name { get; }
    public string Description { get; }
    public string[] DependsOn { get; }
    public CommandArgType Type { get; }
}