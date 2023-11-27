namespace SpinRallyBot;

public static class Constants {
    public const string RttwUrl = "https://r.ttw.ru";
    public static readonly DateTimeOffset ApplicationStartDate = DateTimeOffset.UtcNow;
    public static readonly TimeZoneInfo RussianTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Russian Standard Time");
}