namespace SpinRallyBot;

public static class HostExtensions {
    public static IHost MigrateDatabase(this IHost host) {
        using IServiceScope scope = host.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.Database.Migrate();
        return host;
    }
}
