namespace SpinRallyBot.Events;

public record PlayerRatingChanged(string PlayerUrl, float OldRating, uint OldPosition);