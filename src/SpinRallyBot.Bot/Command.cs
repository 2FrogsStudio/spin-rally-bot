using SpinRallyBot.Attributes;

namespace SpinRallyBot;

public enum Command {
    Unknown,

    [Command("/help",
        Description = "Show this help",
        IsInitCommand = true)]
    Help,
    
    [Command("/start",
        Description = "Start Spin Rally Bot",
        IsInlineCommand = false)]
    Start
}
