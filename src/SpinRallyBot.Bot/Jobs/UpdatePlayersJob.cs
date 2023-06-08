using MassTransit.Scheduling;

namespace SpinRallyBot.Jobs;

public record UpdatePlayersJob;

public class UpdatePlayersJobSchedule : DefaultRecurringSchedule {
    public UpdatePlayersJobSchedule() {
        TimeZoneId = TimeZoneInfo.Utc.Id;
        // todo: pass through configuration
        CronExpression = "0 0 8-20/4 1/1 * ? *"; // every 4th hour from 8 through 20 
        // CronExpression = "0 0/1 * 1/1 * ? *";
    }
}

public class UpdatePlayersJobConsumer : IConsumer<UpdatePlayersJob> {
    private readonly AppDbContext _db;
    private readonly IScopedMediator _mediator;

    public UpdatePlayersJobConsumer(AppDbContext db, IScopedMediator mediator) {
        _db = db;
        _mediator = mediator;
    }

    public async Task Consume(ConsumeContext<UpdatePlayersJob> context) {
        var cancellationToken = context.CancellationToken;
        var playerUrls = await _db.Players
            // update all players to get actual info in find dialog too 
            // .Where(p => p.Subscriptions.Count > 0)
            .Select(p => p.PlayerUrl)
            .ToArrayAsync(cancellationToken);

        var exceptions = new List<Exception>();

        foreach (var playerUrl in playerUrls)
            try {
                await _mediator.Send(new UpdatePlayer(
                    playerUrl,
                    true
                ), cancellationToken);
            } catch (Exception ex) {
                exceptions.Add(ex);
            }

        if (exceptions.Count > 0) {
            throw new AggregateException("Encountered errors while trying to update players.", exceptions);
        }
    }
}

public class UpdatePlayersJobConsumerDefinition : ConsumerDefinition<UpdatePlayersJobConsumer> {
    public UpdatePlayersJobConsumerDefinition() {
        ConcurrentMessageLimit = 1;
    }
}