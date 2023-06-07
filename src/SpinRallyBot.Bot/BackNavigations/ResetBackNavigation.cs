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

        var cancellationToken = context.CancellationToken;
        var entity = await _db.BackNavigations.FindAsync(new object[] { userId, chatId }, cancellationToken)
                     ?? new BackNavigationEntity { UserId = userId, ChatId = chatId };

        entity.Data = JsonSerializer.Serialize(new BackNavigation[] {
            new(Guid.NewGuid(), "↩︎ Меню", new NavigationData.CommandData(Command.Start))
        });

        if (_db.Entry(entity).State is EntityState.Detached) {
            _db.BackNavigations.Add(entity);
        }

        await _db.SaveChangesAsync(cancellationToken);
    }
}