using AWS.Dto;
using AWS.Services;
using Microsoft.Extensions.Configuration;
using System.Text.Json;

var config = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .Build();

var awsSettings = new AwsSettings();

config.GetSection("AWS").Bind(awsSettings);


await RunMessagingFlowAsync();

// --------------------------------------------------------------------------------------------------

async Task RunMessagingFlowAsync()
{
    // 1. Publicar mensagem no SNS
    var publisher = new SnsPublisher(config);

    var meuPedido = new Produto
    {
        Id = 123,
        Nome = "Notebook Dell",
        Valor = 150.75m,
    };

    string jsonMessage = JsonSerializer.Serialize(meuPedido);

    await publisher.PublishAsync(jsonMessage);

    // 2. Receber mensagem do SQS
    var consumer = new SqsConsumer(config);

    await consumer.ReceiveMessageFromSqsAsync();
}





