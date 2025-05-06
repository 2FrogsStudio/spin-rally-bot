namespace SpinRallyBot.PipelineStateMachine;

public record RemovePipelineState(long UserId, long ChatId);

public class RemovePipelineStateConsumer(AppDbContext db) : IMediatorConsumer<RemovePipelineState> {
    public async Task Consume(ConsumeContext<RemovePipelineState> context) {
        CancellationToken cancellationToken = context.CancellationToken;
        var entity = await db.FindAsync<PipelineStateEntity>([
            context.Message.UserId,
            context.Message.ChatId
        ], cancellationToken);
        if (entity is null) {
            return;
        }

        db.Remove(entity);
        await db.SaveChangesAsync(cancellationToken);
    }
}
