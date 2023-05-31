namespace SpinRallyBot.Models;

[PrimaryKey(nameof(PlayerUrl))]
public class PlayerEntity {
    public string PlayerUrl { get; set; } = null!;
    public string Fio { get; set; } = null!;

    public ICollection<SubscriptionEntity> Subscriptions { get; set; } = null!;
}