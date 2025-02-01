using System.ComponentModel.DataAnnotations;

namespace SpinRallyBot.Models;

[PrimaryKey(nameof(PlayerUrl))]
public class PlayerEntity : IDatedEntity {
    [MaxLength(20)]
    public string PlayerUrl { get; set; } = string.Empty;
    [MaxLength(100)]
    public string Fio { get; set; } = string.Empty;
    public float Rating { get; set; }
    public uint Position { get; set; }

    public ICollection<SubscriptionEntity> Subscriptions { get; set; } = new List<SubscriptionEntity>();

    public DateTimeOffset Created { get; set; }
    public DateTimeOffset Updated { get; set; }
}
