using KafkaFlow;
using KafkaFlow.Serializer;
using KafkaFlow.TypedHandler;
using Shared.Messages;
using Tarifa.Worker.Consumers;
using Tarifa.Worker.Domain.Services;
using Tarifa.Worker.Infrastructure;
using Tarifa.Worker.Producers;

public class Program
{
    public static async Task Main(string[] args)
    {
        var host = Host.CreateDefaultBuilder(args)
            .ConfigureServices((hostContext, services) =>
            {
                var config = hostContext.Configuration;
                services.AddSingleton<ITarifaRepository, TarifaRepository>();
                services.AddSingleton<ITarifaService, TarifaService>();
                services.AddSingleton<TarifaProducer>();
                services.AddSingleton<DapperContext>();

                services.AddKafka(kafka => kafka
                    .AddCluster(cluster => cluster
                        .WithBrokers(new[] { config["Kafka:BootstrapServers"] })
                        .AddConsumer(consumer => consumer
                            .Topic(config["Kafka:TopicTransferencias"])
                            .WithGroupId("tarifa-worker")
                            .WithBufferSize(100)
                            .WithWorkersCount(1)
                            .WithAutoOffsetReset(AutoOffsetReset.Earliest)
                            .AddMiddlewares(m => m
                                .AddDeserializer<JsonCoreDeserializer>()
                                .AddTypedHandlers(h => h.AddHandler<TransferenciaConsumer>())
                            )
                        )
                        .AddProducer<TarifaRealizadaMessage>(p => p
                            .DefaultTopic(config["Kafka:TopicTarifas"])
                            .AddMiddlewares(m => m.AddSerializer<JsonCoreSerializer>())
                        )
                    )
                );
            })
            .ConfigureLogging(logging =>
            {
                logging.ClearProviders();
                logging.AddConsole();
                logging.SetMinimumLevel(LogLevel.Debug);
            })
            .Build();

        await host.Services.CreateKafkaBus().StartAsync();

        await host.RunAsync();
    }
}
