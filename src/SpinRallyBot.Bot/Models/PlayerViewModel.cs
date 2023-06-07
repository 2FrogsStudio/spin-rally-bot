namespace SpinRallyBot.Models;

public record PlayerViewModel(string PlayerUrl, string Fio, float Rating, uint Position, DateTimeOffset Updated);