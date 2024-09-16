using MassTransit.RabbitMqTransport.Configuration;
using RabbitMQ.Client;

namespace MassTransit.Repro;

public class StoreBookEndpointDefinition : IEndpointDefinition<IStoreBook>
{
    public bool IsTemporary => false;
    public int? PrefetchCount => default;
    public int? ConcurrentMessageLimit => default;
    public bool ConfigureConsumeTopology => false;

    private readonly IConsumer<IStoreBook> _consumer;

    public StoreBookEndpointDefinition(IConsumer<IStoreBook> consumer)
    {
        _consumer = consumer;
    }

    public string GetEndpointName(IEndpointNameFormatter formatter)
    {
        return GetRoutingKey();
    }

    public void Configure<T>(T configurator) where T : IReceiveEndpointConfigurator
    {
        // https://masstransit-project.com/advanced/middleware/circuit-breaker.html
        configurator.UseCircuitBreaker(config =>
        {
            config.TrackingPeriod = TimeSpan.FromMinutes(1); // The time window we use to measure fail ratio
            config.TripThreshold = 10; // If 10 percent fails; we slow down the consumption of messages
        });

        if (!(configurator is RabbitMqReceiveEndpointConfiguration rabbitMqConfigurator))
            throw new NotSupportedException(
                "The StoreBookEndpointDefinition is only available for RabbitMQ implementations.");
        
        rabbitMqConfigurator.ConfigureConsumeTopology = false;
        rabbitMqConfigurator.Durable = true;
        rabbitMqConfigurator.AutoDelete = false;

        // In case a message send does not match any active receive endpoint; we route it to the NotDelivered queue.
        if (rabbitMqConfigurator.Topology.Publish.GetMessageTopology<IStoreBook>() is
            IRabbitMqMessagePublishTopologyConfigurator<IStoreBook> publishTopologyConfigurator)
        {
            publishTopologyConfigurator.BindAlternateExchangeQueue("Cannot store book");
        }

        // Binds the connection to the most specific routing key
        rabbitMqConfigurator.Bind<IStoreBook>((cfg) =>
        {
            cfg.RoutingKey = GetRoutingKey();
            cfg.ExchangeType = ExchangeType.Direct;
        });

        // The (injected) consumer will be used to handle received messages
        rabbitMqConfigurator.Consumer(() => _consumer);
    }

    private string GetRoutingKey()
    {
        return "StoreBook";
    }
}