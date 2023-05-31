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
        var state = context.Message;

        var entity = await _db.PipelineState.FindAsync(new object[] {
            state.UserId,
            state.ChatId
        }, cancellationToken);

        entity ??= new PipelineState {
            UserId = state.UserId,
            ChatId = state.ChatId
        };

        entity.Data = JsonSerializer.Serialize(state.Data);

        var entityState = _db.Entry(entity).State;
        if (entityState is EntityState.Unchanged) {
            return;
        }

        if (entityState is EntityState.Detached) {
            _db.PipelineState.Add(entity);
        } else if (entityState is EntityState.Modified) {
            _db.PipelineState.Update(entity);
        }

        await _db.SaveChangesAsync(cancellationToken);
    }
}