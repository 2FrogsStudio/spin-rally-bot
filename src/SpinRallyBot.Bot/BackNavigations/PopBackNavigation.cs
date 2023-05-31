namespace SpinRallyBot.BackNavigations;

public record PopBackNavigation(long UserId, long ChatId, Guid Guid);

public record EmptyNavigation;

public class PopBackNavigationConsumer : IMediatorConsumer<PopBackNavigation> {
    private readonly AppDbContext _db;

    public PopBackNavigationConsumer(AppDbContext db) {
        _db = db;
    }

    public async Task Consume(ConsumeContext<PopBackNavigation> context) {
        var query = context.Message;

        var entity = await _db.BackNavigations.FindAsync(query.UserId, query.ChatId);

        if (string.IsNullOrEmpty(entity?.Data)
            || JsonSerializer.Deserialize<List<BackNavigations.BackNavigation>>(entity.Data) is not { } list) {
            await context.RespondAsync(new EmptyNavigation());
            return;
        }

        var findIndex = list.FindIndex(navigation => navigation.Guid == query.Guid);

        var result = list[findIndex];

        if (findIndex + 1 < list.Count) {
            list.RemoveRange(findIndex, list.Count - findIndex);
        }

        entity.Data = JsonSerializer.Serialize(list);
        _db.BackNavigations.Update(entity);
        await _db.SaveChangesAsync(context.CancellationToken);
        await context.RespondAsync(result);
    }
}