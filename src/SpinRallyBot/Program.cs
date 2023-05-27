using SpinRallyBot;

CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;

var host = Host.CreateApplicationBuilder(args)
    .AddLogging()
    .AddMemoryCache()
    .AddDatabase()
    .AddMassTransit()
    .AddTelegramBot()
    .Build();

host
    .MigrateDatabase()
    .Run();
