namespace SpinRallyBot.PipelineStateMachine;

public record SetPipelineData(long UserId, long ChatId, PipelineData Data);

public class SetPipelineDataConsumer(AppDbContext db) : IMediatorConsumer<SetPipelineData> {
    public async Task Consume(ConsumeContext<SetPipelineData> context) {
        CancellationToken cancellationToken = context.CancellationToken;
        SetPipelineData pipelineState = context.Message;

        PipelineStateEntity entity = await db.PipelineState.FindAsync([
                                         pipelineState.UserId,
                                         pipelineState.ChatId
                                     ], cancellationToken)
                                     ?? new PipelineStateEntity {
                                         UserId = pipelineState.UserId,
                                         ChatId = pipelineState.ChatId
                                     };

        entity.Data = JsonSerializer.Serialize(pipelineState.Data);

        if (db.Entry(entity).State is EntityState.Detached) {
            db.Add(entity);
        }

        await db.SaveChangesAsync(context.CancellationToken);
    }
}
