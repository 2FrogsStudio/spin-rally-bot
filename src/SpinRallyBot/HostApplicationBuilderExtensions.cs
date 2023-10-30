using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Quartz;
using SpinRallyBot.Events.PlayerRatingChangedConsumers;
using SpinRallyBot.Serilog;
using SpinRallyBot.Subscriptions;

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
                    typeof(AddSubscriptionConsumer).Assembly);
                if (builder.Configuration["AMQP_URI"] is { } amqpUri) {
                    x.UsingRabbitMq((context, cfg) => {
                        cfg.Host(amqpUri);
                        ConfigureNewtonsoft(cfg);
                        cfg.ConfigureEndpoints(context);
                    });
                } else {
                    x.UsingInMemory((context, cfg) => {
                        ConfigureNewtonsoft(cfg);
                        cfg.ConfigureEndpoints(context);
                    });
                }
            })
            .AddMediator(x => {
                x.AddConsumers(type => type.IsAssignableToGenericType(typeof(IMediatorConsumer<>)),
                    typeof(NotifySubscribersPlayerRatingChangedConsumer).Assembly);
            });
        return builder;
    }

    private static void ConfigureNewtonsoft(IBusFactoryConfigurator cfg) {
        cfg.UseNewtonsoftJsonSerializer();
        cfg.ConfigureNewtonsoftJsonSerializer(_ => new JsonSerializerSettings {
            NullValueHandling = NullValueHandling.Include,
            ContractResolver = new CamelCasePropertyNamesContractResolver {
                IgnoreSerializableAttribute = true,
                IgnoreShouldSerializeMembers = true
            },
            DateFormatHandling = DateFormatHandling.IsoDateFormat,
            DateTimeZoneHandling = DateTimeZoneHandling.Unspecified
        });
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