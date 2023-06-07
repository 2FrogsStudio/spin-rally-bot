namespace SpinRallyBot.Events;

public record PlayerRatingChanged(string PlayerUrl, float OldRating, float NewRating, uint OldPosition,
    float NewPosition);