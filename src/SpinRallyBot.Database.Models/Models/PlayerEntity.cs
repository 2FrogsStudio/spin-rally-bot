namespace SpinRallyBot.Models;

[PrimaryKey(nameof(PlayerUrl))]
public class PlayerEntity : IDatedEntity {
    public string PlayerUrl { get; set; } = null!;
    public string Fio { get; set; } = null!;
    public float Rating { get; set; }
    public uint Position { get; set; }

    public ICollection<SubscriptionEntity> Subscriptions { get; set; } = null!;

    public DateTimeOffset Created { get; set; }
    public DateTimeOffset Updated { get; set; }
}