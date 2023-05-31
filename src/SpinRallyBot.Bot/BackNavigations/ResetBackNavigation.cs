namespace SpinRallyBot.BackNavigations;

public record ResetBackNavigation(long UserId, long ChatId);

public class ResetBackNavigationConsumer : IMediatorConsumer<ResetBackNavigation> {
    private readonly AppDbContext _db;

    public ResetBackNavigationConsumer(AppDbContext db) {
        _db = db;
    }

    public async Task Consume(ConsumeContext<ResetBackNavigation> context) {
        var userId = context.Message.UserId;
        var chatId = context.Message.ChatId;

        var entity = await _db.BackNavigations.FindAsync(userId, chatId)
                     ?? new Models.BackNavigationEntity { UserId = userId, ChatId = chatId };

        entity.Data = JsonSerializer.Serialize(new BackNavigations.BackNavigation[] {
            new(Guid: Guid.NewGuid(), Name: "≡ Меню", Data: new NavigationData.CommandData(Command.Start))
        });

        switch (_db.Entry(entity).State) {
            case EntityState.Unchanged:
                return;
            case EntityState.Detached:
                _db.BackNavigations.Add(entity);
                break;
            case EntityState.Modified:
                _db.BackNavigations.Update(entity);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        await _db.SaveChangesAsync(context.CancellationToken);
    }
}