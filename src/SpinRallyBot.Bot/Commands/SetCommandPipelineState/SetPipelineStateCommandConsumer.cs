using SpinRallyBot.Models;

namespace SpinRallyBot.Commands.SetCommandPipelineState;

public class SetPipelineStateCommandConsumer : IConsumer<SetPipelineStateCommand>, IMediatorConsumer {
    private readonly AppDbContext _db;

    public SetPipelineStateCommandConsumer(AppDbContext db) {
        _db = db;
    }

    public async Task Consume(ConsumeContext<SetPipelineStateCommand> context) {
        var entity = await _db.PipelineStates.FirstOrDefaultAsync(context.CancellationToken);
        if (entity is not null) {
            _db.PipelineStates.Remove(entity);
        }

        await _db.PipelineStates.AddAsync(new PipelineState {
            Command = context.Message.Command.ToString()
        }, context.CancellationToken);

        await _db.SaveChangesAsync(context.CancellationToken);
    }
}