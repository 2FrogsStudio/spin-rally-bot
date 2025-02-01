namespace SpinRallyBot.BackNavigations;

public record PushBackNavigation(long UserId, long ChatId, Guid Guid, string Name, NavigationData Data);

public class PushBackNavigationConsumer : IMediatorConsumer<PushBackNavigation> {
    private readonly AppDbContext _db;

    public PushBackNavigationConsumer(AppDbContext db) {
        _db = db;
    }

    public async Task Consume(ConsumeContext<PushBackNavigation> context) {
        if (context.Message is not {
                ChatId: var chatId,
                UserId: var userId,
                Guid: var guid,
                Name: var name,
                Data: var data
            }) {
            return;
        }

        CancellationToken cancellationToken = context.CancellationToken;

        BackNavigationEntity entity =
            await _db.BackNavigations.FindAsync([userId, chatId], cancellationToken)
            ?? new BackNavigationEntity {
                UserId = userId,
                ChatId = chatId
            };

        List<BackNavigation> backNavigations = string.IsNullOrEmpty(entity.Data)
            ? []
            : JsonSerializer.Deserialize<List<BackNavigation>>(entity.Data) ?? [];

        backNavigations.Add(new BackNavigation(guid, name, data));

        entity.Data = JsonSerializer.Serialize(backNavigations);

        if (_db.Entry(entity).State is EntityState.Detached) {
            _db.BackNavigations.Add(entity);
        }

        await _db.SaveChangesAsync(cancellationToken);
    }
}
