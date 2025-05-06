using System.ComponentModel.DataAnnotations;

namespace SpinRallyBot.Models;

[PrimaryKey(nameof(ChatId), nameof(PlayerUrl))]
public class SubscriptionEntity {
    public long ChatId { get; set; }

    [MaxLength(20)]
    [Required]
    public required string PlayerUrl { get; set; }

    public PlayerEntity Player { get; set; } = new();
}
