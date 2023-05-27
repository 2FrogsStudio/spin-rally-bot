namespace SpinRallyBot;

public abstract class AppDbContext : DbContext {
    protected AppDbContext(DbContextOptions options) : base(options) { }
}
