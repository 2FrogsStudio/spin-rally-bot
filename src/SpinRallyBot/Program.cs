using SpinRallyBot;

CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;

IHost host = Host.CreateApplicationBuilder(args)
    .AddLogging()
    .AddMemoryCache()
    .AddDatabase()
    .AddQuartz()
    .AddMassTransit()
    .AddTelegramBot()
    .AddTtwClient()
    .Build();

host
    .MigrateDatabase()
    .Run();
