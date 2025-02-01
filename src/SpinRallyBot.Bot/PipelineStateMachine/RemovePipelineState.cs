namespace SpinRallyBot.PipelineStateMachine;

public record RemovePipelineState(long UserId, long ChatId);

public class RemovePipelineStateConsumer(AppDbContext db) : IMediatorConsumer<RemovePipelineState> {
    private readonly AppDbContext _db = db;

    public async Task Consume(ConsumeContext<RemovePipelineState> context) {
        CancellationToken cancellationToken = context.CancellationToken;
        var entity = await _db.FindAsync<PipelineStateEntity>([
            context.Message.UserId,
            context.Message.ChatId
        ], cancellationToken);
        if (entity is null) {
            return;
        }

        _db.Remove(entity);
        await _db.SaveChangesAsync(cancellationToken);
    }
}
