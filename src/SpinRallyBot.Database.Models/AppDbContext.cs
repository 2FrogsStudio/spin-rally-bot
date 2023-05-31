namespace SpinRallyBot;

public abstract class AppDbContext : DbContext {
    protected AppDbContext(DbContextOptions options) : base(options) { }
    public DbSet<PipelineState> PipelineState { get; set; } = null!;
    public DbSet<Subscription> Subscriptions { get; set; } = null!;
}