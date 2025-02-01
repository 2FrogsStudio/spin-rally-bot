namespace SpinRallyBot;

public class SqliteDbContext : AppDbContext {
    public SqliteDbContext(DbContextOptions<SqliteDbContext> options) : base(options) { }
}
