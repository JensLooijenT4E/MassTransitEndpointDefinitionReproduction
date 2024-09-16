using MassTransit;
using MassTransit.Configuration;
using MassTransit.RabbitMqTransport.Configuration;
using MassTransit.Repro;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddTransient<IBookService, BookService>();
builder.Services.AddTransient<StoreBookEndpointDefinition>();
builder.Services.AddTransient<IConsumer<IStoreBook>, StoreBookConsumer>();

builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<StoreBookConsumer>();

    x.SetBusFactory(new RabbitMqRegistrationBusFactory((context, configurator) =>
    {
        configurator.Host(new Uri("rabbitmq://localhost"), host =>
        {
            host.Username("UserName");
            host.Password("Password");
        });

        configurator.PrefetchCount = 16;
        configurator.UseMessageRetry(retry => retry.Immediate(2));

        configurator.ConfigureEndpoints(context);
    }));

    //Works in 8.0.15, but not in 8.1.0 and beyond
    x.RegisterEndpoint(typeof(StoreBookEndpointDefinition));
    // No way to get an IRegistration?
    //x.AddEndpoint<StoreBookEndpointDefinition, IStoreBook>();
});

var app = builder.Build();

app.UseHttpsRedirection();
app.MapControllers();

app.Run();
