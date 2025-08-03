using Amazon.SQS;
using Amazon.SQS.Model;
using AWS.Dto;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace AWS.Services
{
    public class SqsConsumer : ISqsConsumer
    {
        private readonly AmazonSQSClient _sqsClient;
        private readonly AwsSettings _awsSettings;
        private string _queueUrl;

        public SqsConsumer(AmazonSQSClient sqsClient, IOptions<AwsSettings> awsOptions)
        {
            _sqsClient = sqsClient;
            _awsSettings = awsOptions.Value;
        }

        public async Task ReceiveMessagesAsync()
        {
            Console.WriteLine("Aguardando 5 segundos para a mensagem chegar na fila...");
            await Task.Delay(5000);

            Console.WriteLine("Recebendo mensagens da fila SQS...");

            try
            {
                if (string.IsNullOrEmpty(_queueUrl))
                {
                    var queueUrlResponse = await _sqsClient.GetQueueUrlAsync(new GetQueueUrlRequest { QueueName = _awsSettings.SqsQueueName });
                    _queueUrl = queueUrlResponse.QueueUrl;
                }

                var receiveRequest = new ReceiveMessageRequest
                {
                    QueueUrl = _queueUrl,
                    MaxNumberOfMessages = 1,
                    WaitTimeSeconds = 10
                };

                var receiveResponse = await _sqsClient.ReceiveMessageAsync(receiveRequest);

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
                            Console.WriteLine($"Valor: {produto.Valor:C}");
                            Console.WriteLine("Itens:");
                        }
                        catch (Exception)
                        {
                            Console.WriteLine("Não foi possível desserializar a mensagem do SNS.");
                        }

                        await _sqsClient.DeleteMessageAsync(new DeleteMessageRequest
                        {
                            QueueUrl = _queueUrl,
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