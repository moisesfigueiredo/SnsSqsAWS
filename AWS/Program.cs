using AWS.Configuration;
using AWS.Dto;
using AWS.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder(args);

// Configuração do Options Pattern
builder.Services.Configure<AwsSettings>(
    builder.Configuration.GetSection("AWS"));

// Registro dos clientes AWS para Injeção de Dependência
builder.Services.RegisterAwsClients();

// Registro dos serviços (com as interfaces)
builder.Services.AddDependencyInjectionConfiguration();

using var host = builder.Build();

await RunMessagingFlowAsync(host.Services);

// ==================================================================================================

async Task RunMessagingFlowAsync(IServiceProvider services)
{
    var publisher = services.GetRequiredService<ISnsPublisher>();
    var consumer = services.GetRequiredService<ISqsConsumer>();

    // 1. Publicar mensagem no SNS
    var meuPedido = new Produto
    {
        Id = 123,
        Nome = "Notebook Dell",
        Valor = 150.75m,
    };

    await publisher.PublishAsync(meuPedido);

    // 2. Receber mensagem do SQS
    await consumer.ReceiveMessagesAsync();
}