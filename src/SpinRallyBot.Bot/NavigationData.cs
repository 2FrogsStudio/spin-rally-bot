using System.Text.Json.Serialization;

// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable UnusedMember.Global
// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global

namespace SpinRallyBot;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "$t")]
[JsonDerivedType(typeof(CommandData), "C")]
[JsonDerivedType(typeof(PipelineData), "P")]
[JsonDerivedType(typeof(ActionData), "A")]
[JsonDerivedType(typeof(BackData), "B")]
public abstract class NavigationData {
    [JsonPolymorphic(TypeDiscriminatorPropertyName = "$t")]
    [JsonDerivedType(typeof(CommandData), "C")]
    public class CommandData : NavigationData {
        public CommandData() { }

        public CommandData(Command command, string? data = null) {
            Command = command;
            Data = data;
        }

        [JsonPropertyName("C")] public Command Command { get; init; }

        [JsonPropertyName("D")] public string? Data { get; init; }
    }

    [JsonPolymorphic(TypeDiscriminatorPropertyName = "$t")]
    [JsonDerivedType(typeof(PipelineData), "P")]
    public class PipelineData : NavigationData {
        public PipelineData() { }

        public PipelineData(Pipeline pipeline, string? data = null) {
            Pipeline = pipeline;
            Data = data;
        }

        [JsonPropertyName("P")] public Pipeline Pipeline { get; init; }

        [JsonPropertyName("D")] public string? Data { get; init; }
    }

    [JsonPolymorphic(TypeDiscriminatorPropertyName = "$t")]
    [JsonDerivedType(typeof(ActionData), "A")]
    public class ActionData : NavigationData {
        public ActionData() { }

        public ActionData(Actions action, string? data = null) {
            Action = action;
            Data = data;
        }

        [JsonPropertyName("A")] public Actions Action { get; init; }

        [JsonPropertyName("D")] public string? Data { get; init; }
    }

    [JsonPolymorphic(TypeDiscriminatorPropertyName = "$t")]
    [JsonDerivedType(typeof(BackData), "B")]
    public class BackData : NavigationData {
        public BackData() { }

        public BackData(Guid guid) {
            Guid = guid;
        }

        [JsonPropertyName("G")] public Guid Guid { get; init; }
    }
}