using KafkaFlow;
using KafkaFlow.Serializer;
using KafkaFlow.TypedHandler;
using Tarifa.Worker.Consumers;
using Tarifa.Worker.Domain.Services;
using Tarifa.Worker.Infrastructure;
using Tarifa.Worker.Producers;

Host.CreateDefaultBuilder(args)
    .ConfigureServices((hostContext, services) =>
    {
        var config = hostContext.Configuration;

        services.AddSingleton<ITarifaRepository, TarifaRepository>();
        services.AddSingleton<ITarifaService, TarifaService>();
        services.AddSingleton<TarifaProducer>();

        services.AddKafka(kafka => kafka
            .AddCluster(cluster => cluster
                .WithBrokers(new[] { config["Kafka:BootstrapServers"] })

                // Consumer: recebe transferências realizadas
                .AddConsumer(consumer => consumer
                    .Topic(config["Kafka:TopicTransferencias"])
                    .WithGroupId("tarifa-worker")
                    .WithBufferSize(100)
                    .WithWorkersCount(1)
                    .AddMiddlewares(m => m
                        .AddDeserializer<JsonCoreDeserializer>()
                        .AddTypedHandlers(h => h.AddHandler<TransferenciaConsumer>())
                    )
                )

                // Producer: publica tarifações realizadas
                .AddProducer<TarifaRealizadaMessage>(p => p
                    .DefaultTopic(config["Kafka:TopicTarifas"])
                    .AddMiddlewares(m => m.AddSerializer<JsonCoreSerializer>())
                )
            )
        );
    })
    .ConfigureLogging(logging =>
    {
        logging.AddConsole();
    })
    .Build()
    .Run();