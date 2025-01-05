namespace SpinRallyBot.PipelineStateMachine;

public record GetPipelineData(long UserId, long ChatId);

public record NoPipelineStateResult;

public class GetPipelineDataRequestConsumer : IMediatorConsumer<GetPipelineData> {
    private readonly AppDbContext _db;

    public GetPipelineDataRequestConsumer(AppDbContext db) {
        _db = db;
    }

    public async Task Consume(ConsumeContext<GetPipelineData> context) {
        CancellationToken cancellationToken = context.CancellationToken;
        PipelineStateEntity? pipelineState = await _db.PipelineState.FindAsync(new object[] {
            context.Message.UserId,
            context.Message.ChatId
        }, cancellationToken);

        if (pipelineState is not null) {
            await context.RespondAsync(JsonSerializer.Deserialize<PipelineData>(pipelineState.Data)!);
            return;
        }

        await context.RespondAsync(new NoPipelineStateResult());
    }
}
