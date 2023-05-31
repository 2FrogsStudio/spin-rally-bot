namespace SpinRallyBot;

public abstract class AppDbContext : DbContext {
    protected AppDbContext(DbContextOptions options) : base(options) { }
    public DbSet<PipelineStateEntity> PipelineState { get; set; } = null!;
    public DbSet<SubscriptionEntity> Subscriptions { get; set; } = null!;
    public DbSet<BackNavigationEntity> BackNavigations { get; set; } = null!;
}