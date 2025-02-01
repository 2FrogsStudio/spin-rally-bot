using System.ComponentModel.DataAnnotations;

namespace SpinRallyBot.Models;

[PrimaryKey(nameof(UserId), nameof(ChatId))]
public class BackNavigationEntity {
    public long UserId { get; set; }
    public long ChatId { get; set; }
    [MaxLength(10000)]
    public string Data { get; set; } = string.Empty;
}
