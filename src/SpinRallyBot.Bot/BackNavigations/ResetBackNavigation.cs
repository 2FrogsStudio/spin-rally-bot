namespace SpinRallyBot.BackNavigations;

public record ResetBackNavigation(long UserId, long ChatId);

public class ResetBackNavigationConsumer : IMediatorConsumer<ResetBackNavigation> {
    private readonly AppDbContext _db;

    public ResetBackNavigationConsumer(AppDbContext db) {
        _db = db;
    }

    public async Task Consume(ConsumeContext<ResetBackNavigation> context) {
        long userId = context.Message.UserId;
        long chatId = context.Message.ChatId;

        CancellationToken cancellationToken = context.CancellationToken;
        BackNavigationEntity entity =
            await _db.BackNavigations.FindAsync([userId, chatId], cancellationToken)
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
