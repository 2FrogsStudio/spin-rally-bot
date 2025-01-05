namespace SpinRallyBot.Models;

public record PlayerInfo(string PlayerUrl, string Fio, float Rating, uint Position) : Player(PlayerUrl, Fio);
