using System.Text.Json;

namespace SpinRallyBot.Bot.Tests;

public class CallbackDataSerializationTests {
    [Fact]
    public void SerializeForCommandAndDeserializeForCommand() {
        var callbackData = new CallbackData.CommandData(Command.Find, "/players/?id=52a31ad");

        var json = JsonSerializer.Serialize(callbackData);
        var data = JsonSerializer.Deserialize<CallbackData>(json);

        json.Length.Should().BeLessOrEqualTo(64);
        data.Should().BeOfType<CallbackData.CommandData>()
            .Which.Should().Match<CallbackData.CommandData>(f =>
                f.Command == Command.Find
                && f.Data == "/players/?id=52a31ad");
    }

    [Fact]
    public void SerializeCallbackDataAndDeserializeForCommand() {
        CallbackData callbackData = new CallbackData.CommandData(Command.Find, "/players/?id=52a31ad");

        var json = JsonSerializer.Serialize(callbackData);
        var data = JsonSerializer.Deserialize<CallbackData>(json);

        json.Length.Should().BeLessOrEqualTo(64);
        data.Should().BeOfType<CallbackData.CommandData>()
            .Which.Should().Match<CallbackData.CommandData>(f =>
                f.Command == Command.Find
                && f.Data == "/players/?id=52a31ad");
    }
}