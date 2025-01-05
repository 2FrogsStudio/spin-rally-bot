using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace SpinRallyBot;

public abstract class AppDbContext(DbContextOptions options) : DbContext(options) {
    public DbSet<PipelineStateEntity> PipelineState { get; set; } = null!;
    public DbSet<SubscriptionEntity> Subscriptions { get; set; } = null!;
    public DbSet<BackNavigationEntity> BackNavigations { get; set; } = null!;
    public DbSet<PlayerEntity> Players { get; set; } = null!;

    public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess,
        CancellationToken cancellationToken = default) {
        FillDatedEntities();
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    public override int SaveChanges(bool acceptAllChangesOnSuccess) {
        FillDatedEntities();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    private void FillDatedEntities() {
        var nowLazy = new Lazy<DateTimeOffset>(() => DateTimeOffset.UtcNow);
        foreach (EntityEntry<IDatedEntity> entry in ChangeTracker.Entries<IDatedEntity>()) {
            IDatedEntity entity = entry.Entity;
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

    protected override void OnModelCreating(ModelBuilder modelBuilder) {
        modelBuilder.Entity<PlayerEntity>()
            .HasMany(e => e.Subscriptions)
            .WithOne(e => e.Player)
            .HasForeignKey(e => e.PlayerUrl)
            .IsRequired();

        base.OnModelCreating(modelBuilder);
    }
}
