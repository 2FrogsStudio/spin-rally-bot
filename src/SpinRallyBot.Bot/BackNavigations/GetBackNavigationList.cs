namespace SpinRallyBot.BackNavigations;

public record GetBackNavigationList(long UserId, long ChatId);

public record GetBackNavigationResult(string Name, Guid Guid);

public class GetBackNavigationListConsumer : IMediatorConsumer<GetBackNavigationList> {
    private readonly AppDbContext _db;

    public GetBackNavigationListConsumer(AppDbContext db) {
        _db = db;
    }

    public async Task Consume(ConsumeContext<GetBackNavigationList> context) {
        var query = context.Message;

        var entity = await _db.BackNavigations.FindAsync(query.UserId, query.ChatId);

        if (string.IsNullOrEmpty(entity?.Data)
            || JsonSerializer.Deserialize<List<BackNavigations.BackNavigation>>(entity.Data) is not { } list) {
            await context.RespondAsync(new EmptyNavigation());
            return;
        }

        var result = list.Select(n=> new GetBackNavigationResult(n.Name, n.Guid)).Reverse().ToArray();
        await context.RespondAsync(result);
    }
}