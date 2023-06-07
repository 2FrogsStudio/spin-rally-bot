namespace SpinRallyBot.Commands;

public record AddPlayerToDb(Player Player);

public class AddPlayerToDbConsumer : IMediatorConsumer<AddPlayerToDb> {
    private readonly AppDbContext _db;

    public AddPlayerToDbConsumer(AppDbContext db) {
        _db = db;
    }

    public async Task Consume(ConsumeContext<AddPlayerToDb> context) {
        var player = context.Message.Player;
        var playerEntity = await _db.Players.FindAsync(player.PlayerUrl, context.CancellationToken)
                           ?? new PlayerEntity { PlayerUrl = player.PlayerUrl, Fio = player.Fio };

        if (_db.Entry(playerEntity).State is EntityState.Detached) {
            _db.Players.Add(playerEntity);
            await _db.SaveChangesAsync(context.CancellationToken);
        }
    }
}