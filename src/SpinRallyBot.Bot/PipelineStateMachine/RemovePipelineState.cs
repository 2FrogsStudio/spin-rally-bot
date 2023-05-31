using SpinRallyBot.Models;

namespace SpinRallyBot.PipelineStateMachine;

public record RemovePipelineState(long UserId, long ChatId);

public class RemovePipelineStateConsumer : IMediatorConsumer<RemovePipelineState> {
    private readonly AppDbContext _db;

    public RemovePipelineStateConsumer(AppDbContext db) {
        _db = db;
    }

    public async Task Consume(ConsumeContext<RemovePipelineState> context) {
        var cancellationToken = context.CancellationToken;
        var entity = await _db.FindAsync<PipelineState>(new object[] {
            context.Message.UserId,
            context.Message.ChatId
        }, cancellationToken);
        if (entity is not null) {
            _db.PipelineState.Remove(entity);
            await _db.SaveChangesAsync(cancellationToken);
        }
    }
}