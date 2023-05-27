using SpinRallyBot.Attributes;

namespace SpinRallyBot;

public enum Command {
    Unknown,

    [Command("/help",
        Description = "Show this help",
        IsBotInitCommand = true)]
    Help
}
