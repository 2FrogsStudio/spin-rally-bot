using System.ComponentModel.DataAnnotations;

namespace SpinRallyBot.Models;

[PrimaryKey(nameof(ChatId), nameof(PlayerUrl))]
public class SubscriptionEntity {
    public long ChatId { get; set; }
    [MaxLength(20)]
    public string PlayerUrl { get; set; } = null!;

    public PlayerEntity Player { get; set; } = null!;
}
