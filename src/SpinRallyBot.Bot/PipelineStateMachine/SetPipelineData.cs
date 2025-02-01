namespace SpinRallyBot.PipelineStateMachine;

public record SetPipelineData(long UserId, long ChatId, PipelineData Data);

public class SetPipelineDataConsumer(AppDbContext db) : IMediatorConsumer<SetPipelineData> {
    private readonly AppDbContext _db = db;

    public async Task Consume(ConsumeContext<SetPipelineData> context) {
        CancellationToken cancellationToken = context.CancellationToken;
        SetPipelineData pipelineState = context.Message;

        PipelineStateEntity entity = await _db.PipelineState.FindAsync([
                                         pipelineState.UserId,
                                         pipelineState.ChatId
                                     ], cancellationToken)
                                     ?? new PipelineStateEntity {
                                         UserId = pipelineState.UserId,
                                         ChatId = pipelineState.ChatId
                                     };

        entity.Data = JsonSerializer.Serialize(pipelineState.Data);

        if (_db.Entry(entity).State is EntityState.Detached) {
            _db.Add(entity);
        }

        await _db.SaveChangesAsync(context.CancellationToken);
    }
}
