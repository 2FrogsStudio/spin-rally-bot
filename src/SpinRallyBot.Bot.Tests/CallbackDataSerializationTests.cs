using System.Text.Json;

namespace SpinRallyBot.Bot.Tests;

public class CallbackDataSerializationTests {
    [Fact]
    public void SerializeForCommandAndDeserializeForCommand() {
        var callbackData = new NavigationData.CommandData(Command.Find, "/players/?id=52a31ad");

        var json = JsonSerializer.Serialize(callbackData);
        var data = JsonSerializer.Deserialize<NavigationData>(json);

        json.Length.Should().BeLessOrEqualTo(64);
        data.Should().BeOfType<NavigationData.CommandData>()
            .Which.Should().Match<NavigationData.CommandData>(f =>
                f.Command == Command.Find
                && f.Data == "/players/?id=52a31ad");
    }

    [Fact]
    public void SerializeCallbackDataAndDeserializeForCommand() {
        NavigationData navigationData = new NavigationData.CommandData(Command.Find, "/players/?id=52a31ad");

        var json = JsonSerializer.Serialize(navigationData);
        var data = JsonSerializer.Deserialize<NavigationData>(json);

        json.Length.Should().BeLessOrEqualTo(64);
        data.Should().BeOfType<NavigationData.CommandData>()
            .Which.Should().Match<NavigationData.CommandData>(f =>
                f.Command == Command.Find
                && f.Data == "/players/?id=52a31ad");
    }
}