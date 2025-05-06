namespace SpinRallyBot.PipelineStateMachine;

public record GetPipelineData(long UserId, long ChatId);

public record NoPipelineStateResult;

public class GetPipelineDataRequestConsumer(AppDbContext db) : IMediatorConsumer<GetPipelineData> {
    public async Task Consume(ConsumeContext<GetPipelineData> context) {
        CancellationToken cancellationToken = context.CancellationToken;
        PipelineStateEntity? pipelineState = await db.PipelineState.FindAsync([
            context.Message.UserId,
            context.Message.ChatId
        ], cancellationToken);

        if (pipelineState is not null) {
            await context.RespondAsync(JsonSerializer.Deserialize<PipelineData>(pipelineState.Data) ??
                                       throw new NullReferenceException());
            return;
        }

        await context.RespondAsync(new NoPipelineStateResult());
    }
}
