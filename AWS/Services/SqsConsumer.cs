using Amazon.SQS;
using Amazon.SQS.Model;
using AWS.Dto;
using Microsoft.Extensions.Configuration;
using System.Text.Json;
namespace AWS.Services
{
    public class SqsConsumer
    {
        private readonly IConfiguration configuration;

        public SqsConsumer(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public async Task ReceiveMessageFromSqsAsync()
        {
            Console.WriteLine("Aguardando 5 segundos para a mensagem chegar na fila...");
            await Task.Delay(5000);

            Console.WriteLine("Recebendo mensagens da fila SQS...");

            try
            {
                var awsServiceUrl = configuration["AWS:ServiceURL"];
                var region = configuration["AWS:Region"];
                var sqsQueueName = configuration["AWS:SqsQueueName"];

                var sqsClient = new AmazonSQSClient(new AmazonSQSConfig
                {
                    ServiceURL = awsServiceUrl,
                    AuthenticationRegion = region
                });

                var queueUrlResponse = await sqsClient.GetQueueUrlAsync(new GetQueueUrlRequest { QueueName = sqsQueueName });
                var queueUrl = queueUrlResponse.QueueUrl;

                var receiveRequest = new ReceiveMessageRequest
                {
                    QueueUrl = queueUrl,
                    MaxNumberOfMessages = 1,
                    WaitTimeSeconds = 10
                };

                var receiveResponse = await sqsClient.ReceiveMessageAsync(receiveRequest);

                if (receiveResponse.Messages.Any())
                {
                    foreach (var message in receiveResponse.Messages)
                    {
                        Console.WriteLine("--- Mensagem Recebida ---");
                        Console.WriteLine($"Body: {message.Body}");

                        try
                        {
                            var snsMessage = JsonSerializer.Deserialize<SnsMessage>(message.Body);

                            Console.WriteLine($"Mensagem Original do SNS: {snsMessage.Message}");

                            var produto = JsonSerializer.Deserialize<Produto>(snsMessage.Message);

                            Console.WriteLine($"--- Produto Recebido ---");
                            Console.WriteLine($"ID do Produto: {produto.Id}");
                            Console.WriteLine($"Nome: {produto.Nome}");
                            Console.WriteLine($"Valor: {produto.Valor:C}"); // Exemplo de formatação monetária
                            Console.WriteLine("Itens:");
                        }
                        catch (Exception)
                        {
                            Console.WriteLine("Não foi possível desserializar a mensagem do SNS.");
                        }

                        await sqsClient.DeleteMessageAsync(new DeleteMessageRequest
                        {
                            QueueUrl = queueUrl,
                            ReceiptHandle = message.ReceiptHandle
                        });

                        Console.WriteLine("Mensagem excluída da fila.");
                    }
                }
                else
                {
                    Console.WriteLine("Nenhuma mensagem recebida.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao receber ou excluir mensagem: {ex.Message}");
            }
        }
    }
}