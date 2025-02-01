namespace SpinRallyBot.BackNavigations;

public record GetBackNavigationList(long UserId, long ChatId);

public record GetBackNavigationResult(string Name, Guid Guid);

public class GetBackNavigationListConsumer(AppDbContext db) : IMediatorConsumer<GetBackNavigationList> {
    private readonly AppDbContext _db = db;

    public async Task Consume(ConsumeContext<GetBackNavigationList> context) {
        GetBackNavigationList query = context.Message;
        CancellationToken cancellationToken = context.CancellationToken;

        BackNavigationEntity? entity =
            await _db.BackNavigations.FindAsync([query.UserId, query.ChatId], cancellationToken);

        if (string.IsNullOrEmpty(entity?.Data)
            || JsonSerializer.Deserialize<List<BackNavigation>>(entity.Data) is not { } list) {
            await context.RespondAsync(new EmptyNavigation());
            return;
        }

        GetBackNavigationResult[] result = list.Select(n => new GetBackNavigationResult(n.Name, n.Guid)).Reverse()
            .ToArray();
        await context.RespondAsync(result);
    }
}
