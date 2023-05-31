namespace SpinRallyBot.Models;

[PrimaryKey(nameof(ChatId), nameof(PlayerUrl))]
public class Subscription {
    public long ChatId { get; set; }
    public string PlayerUrl { get; set; } = null!;
    public string Fio { get; set; } = null!;
}