using SpinRallyBot.Models;

namespace SpinRallyBot.PipelineStateMachine;

public record SetPipelineData(long UserId, long ChatId, PipelineData Data);

public class SetPipelineDataConsumer : IMediatorConsumer<SetPipelineData> {
    private readonly AppDbContext _db;

    public SetPipelineDataConsumer(AppDbContext db) {
        _db = db;
    }

    public async Task Consume(ConsumeContext<SetPipelineData> context) {
        var cancellationToken = context.CancellationToken;
        var pipelineState = context.Message;

        var entity = await _db.PipelineState.FindAsync(new object[] {
                         pipelineState.UserId,
                         pipelineState.ChatId
                     }, cancellationToken)
                     ?? new PipelineStateEntity {
                         UserId = pipelineState.UserId,
                         ChatId = pipelineState.ChatId
                     };

        entity.Data = JsonSerializer.Serialize(pipelineState.Data);

        switch (_db.Entry(entity).State) {
            case EntityState.Unchanged:
                return;
            case EntityState.Detached:
                _db.PipelineState.Add(entity);
                break;
            case EntityState.Modified:
                _db.PipelineState.Update(entity);
                break;
        }

        await _db.SaveChangesAsync(cancellationToken);
    }
}