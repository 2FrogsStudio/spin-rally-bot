using Quartz;
using SpinRallyBot.Events.PullingServiceActivatedConsumers;
using SpinRallyBot.Events.UpdateReceivedConsumers;
using SpinRallyBot.Serilog;

namespace SpinRallyBot;

public static class HostApplicationBuilderExtensions {
    public static HostApplicationBuilder AddLogging(this HostApplicationBuilder builder) {
        builder.Services.AddLogging(loggingBuilder => {
            loggingBuilder.AddConfiguration(builder.Configuration);
            loggingBuilder.ClearProviders();
            loggingBuilder.AddSerilog(new LoggerConfiguration()
                .ReadFrom.Configuration(builder.Configuration)
                // .Destructure.With<IncludePublicNotNullFieldsPolicy>()
                .Destructure.With<SerializeJsonElementPolicy>()
                .CreateLogger());
            loggingBuilder.AddSentry(options => options.Environment = builder.Environment.EnvironmentName);
        });
        return builder;
    }

    public static HostApplicationBuilder AddMemoryCache(this HostApplicationBuilder builder) {
        builder.Services.AddMemoryCache()
            .Configure<MemoryCacheOptions>(builder.Configuration.GetSection("MemoryCache"));
        return builder;
    }

    public static HostApplicationBuilder AddMassTransit(this HostApplicationBuilder builder) {
        builder.Services
            .AddMassTransit(x => {
                x.AddQuartzConsumers();
                x.AddConsumers(type => !type.IsAssignableToGenericType(typeof(IMediatorConsumer<>)),
                    typeof(ShutdownApplicationPullingServiceActivatedConsumer).Assembly);
                if (builder.Configuration["AMQP_URI"] is { } amqpUri) {
                    x.UsingRabbitMq((context, cfg) => {
                        cfg.Host(amqpUri);
                        cfg.ConfigureEndpoints(context);
                    });
                } else {
                    x.UsingInMemory((context, cfg) => cfg.ConfigureEndpoints(context));
                }
            })
            .AddMediator(x => {
                x.AddConsumers(type => type.IsAssignableToGenericType(typeof(IMediatorConsumer<>)),
                    typeof(PublishCommandReceivedUpdateReceivedConsumer).Assembly);
            });
        return builder;
    }

    public static HostApplicationBuilder AddQuartz(this HostApplicationBuilder builder) {
        builder.Services.AddQuartz(q => {
            q.UseMicrosoftDependencyInjectionJobFactory();
            q.UsePersistentStore(s => {
                var provider = builder.Configuration.GetValue("Provider", "Postgres");
                switch (provider) {
                    case "Sqlite":
                        s.UseMicrosoftSQLite(server =>
                            server.ConnectionString = builder.Configuration.GetConnectionString(provider)!);
                        break;
                    case "Postgres":
                        s.UsePostgres(server =>
                            server.ConnectionString = builder.Configuration.GetConnectionString(provider)!);
                        s.UseClustering();
                        break;
                    default:
                        throw new Exception($"Unsupported provider: {provider}");
                }

                s.UseJsonSerializer();
            });
        });
        return builder;
    }
}