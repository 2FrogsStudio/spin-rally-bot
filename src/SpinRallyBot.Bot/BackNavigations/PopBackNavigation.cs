namespace SpinRallyBot.BackNavigations;

public record PopBackNavigation(long UserId, long ChatId, Guid? Guid = null);

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
            || JsonSerializer.Deserialize<List<BackNavigation>>(entity.Data) is not { } list
            || list.Count == 0) {
            await context.RespondAsync(new EmptyNavigation());
            return;
        }

        var findIndex = query.Guid is not null
            ? list.FindIndex(navigation => navigation.Guid == query.Guid)
            : list.Count - 1;

        if (findIndex == -1 || list[findIndex] is not { } result) {
            await context.RespondAsync(new EmptyNavigation());
            return;
        }

        if (findIndex + 1 < list.Count) {
            list.RemoveRange(findIndex, list.Count - findIndex);
        }

        entity.Data = JsonSerializer.Serialize(list);
        await _db.SaveChangesAsync(context.CancellationToken);
        await context.RespondAsync(result);
    }
}