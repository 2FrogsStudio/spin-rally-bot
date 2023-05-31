using SpinRallyBot;

CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;

var host = Host.CreateApplicationBuilder(args)
    .AddLogging()
    .AddMemoryCache()
    .AddDatabase()
    .AddMassTransit()
    .AddTelegramBot()
    .AddTtwClient()
    .Build();

host
    .MigrateDatabase()
    .Run();