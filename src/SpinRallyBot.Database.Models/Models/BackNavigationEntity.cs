namespace SpinRallyBot.Models;

[PrimaryKey(nameof(UserId), nameof(ChatId))]
public class BackNavigationEntity {
    public long UserId { get; set; }
    public long ChatId { get; set; }
    public string Data { get; set; } = null!;
}