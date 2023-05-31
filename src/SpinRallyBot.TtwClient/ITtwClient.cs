namespace SpinRallyBot;

public interface ITtwClient {
    Task<PlayerInfo?> GetPlayerInfo(string playerUrl, CancellationToken cancellationToken);
    Task<Player[]> FindPlayers(string searchQuery, CancellationToken cancellationToken);
}