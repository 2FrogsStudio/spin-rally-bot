using System.Text.Json;

namespace SpinRallyBot.Bot.Tests;

public class CallbackDataSerializationTests {
    [Fact]
    public void SerializeForCommandAndDeserializeForCommand() {
        var callbackData = new NavigationData.CommandData(Command.Find, "/players/?id=52a31ad");

        string json = JsonSerializer.Serialize(callbackData);
        var data = JsonSerializer.Deserialize<NavigationData>(json);

        json.Length.ShouldBeLessThanOrEqualTo(64);
        var commandData = data.ShouldBeOfType<NavigationData.CommandData>();
        commandData.ShouldSatisfyAllConditions(
            () => commandData.Command.ShouldBe(Command.Find),
            () => commandData.Data.ShouldBe("/players/?id=52a31ad")
        );
    }

    [Fact]
    public void SerializeCallbackDataAndDeserializeForCommand() {
        NavigationData navigationData = new NavigationData.CommandData(Command.Find, "/players/?id=52a31ad");

        string json = JsonSerializer.Serialize(navigationData);
        var data = JsonSerializer.Deserialize<NavigationData>(json);

        json.Length.ShouldBeLessThanOrEqualTo(64);
        var commandData = data.ShouldBeOfType<NavigationData.CommandData>();
        commandData.ShouldSatisfyAllConditions(
            () => commandData.Command.ShouldBe(Command.Find),
            () => commandData.Data.ShouldBe("/players/?id=52a31ad")
        );
    }
}
