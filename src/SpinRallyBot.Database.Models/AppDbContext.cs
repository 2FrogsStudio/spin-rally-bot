namespace SpinRallyBot;

public abstract class AppDbContext : DbContext {
    protected AppDbContext(DbContextOptions options) : base(options) {
        SavingChanges += OnSavingChanges;
    }

    public DbSet<PipelineStateEntity> PipelineState { get; set; } = null!;
    public DbSet<SubscriptionEntity> Subscriptions { get; set; } = null!;
    public DbSet<BackNavigationEntity> BackNavigations { get; set; } = null!;
    public DbSet<PlayerEntity> Players { get; set; } = null!;

    private void OnSavingChanges(object? sender, SavingChangesEventArgs e) {
        var nowLazy = new Lazy<DateTimeOffset>(() => DateTimeOffset.UtcNow);
        foreach (var entry in ChangeTracker.Entries<IDatedEntity>()) {
            var entity = entry.Entity;
            switch (entry.State) {
                case EntityState.Added:
                    entity.Created = nowLazy.Value;
                    entity.Updated = nowLazy.Value;
                    break;
                case EntityState.Modified:
                    entity.Updated = nowLazy.Value;
                    break;
            }
        }
    }
}