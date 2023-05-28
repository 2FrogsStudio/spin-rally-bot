namespace SpinRallyBot.Queries.GetCommandPipelineState;

public class GetPipelineStateRequestConsumer : IConsumer<GetPipelineStateQuery>, IMediatorConsumer {
    private readonly AppDbContext _db;

    public GetPipelineStateRequestConsumer(AppDbContext db) {
        _db = db;
    }

    public async Task Consume(ConsumeContext<GetPipelineStateQuery> context) {
        var cancellationToken = context.CancellationToken;
        var entity = await _db.PipelineStates.FirstOrDefaultAsync(cancellationToken);

        if (entity is not null
            && Enum.TryParse(entity.Command, out Command command)) {
            await context.RespondAsync<PipelineStateResult>(new { Command = command });
            return;
        }

        await context.RespondAsync<NoPipelineStateResult>(new { });
    }
}