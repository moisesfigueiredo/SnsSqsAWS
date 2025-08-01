using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;
using Microsoft.Extensions.Configuration;

namespace AWS.Services
{
    public class SnsPublisher
    {
        private readonly IConfiguration configuration;

        public SnsPublisher(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public async Task PublishAsync(string messageBody)
        {
            Console.WriteLine("Publicando mensagem no SNS...");

            var accessKey = configuration["AWS:AccessKey"];
            var secretKey = configuration["AWS:SecretKey"];
            var region = configuration["AWS:Region"];
            var awsServiceUrl = configuration["AWS:ServiceURL"];
            var snsTopicName = configuration["AWS:SnsTopicName"];

            var snsClient = new AmazonSimpleNotificationServiceClient(new AmazonSimpleNotificationServiceConfig
            {
                ServiceURL = awsServiceUrl,
                AuthenticationRegion = region
            });

            var topicArn = await GetTopicArnWithRetry(snsClient, snsTopicName, 5, TimeSpan.FromSeconds(2));

            if (string.IsNullOrEmpty(topicArn))
            {
                Console.WriteLine($"Erro: Tópico SNS '{snsTopicName}' não encontrado. A publicação falhou.");
                return;
            }

            var publishRequest = new PublishRequest
            {
                TopicArn = topicArn,
                Message = messageBody,
                Subject = "Teste de Mensagem"
            };

            try
            {
                var publishResponse = await snsClient.PublishAsync(publishRequest);
                Console.WriteLine($"Mensagem publicada com sucesso! ID da mensagem: {publishResponse.MessageId}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao publicar mensagem: {ex.Message}");
            }
        }

        async Task<string> GetTopicArnWithRetry(AmazonSimpleNotificationServiceClient client, string topicName, int maxRetries, TimeSpan delay)
        {
            for (int i = 0; i < maxRetries; i++)
            {
                try
                {
                    var topicResponse = await client.ListTopicsAsync(new ListTopicsRequest());
                    var foundTopic = topicResponse.Topics.FirstOrDefault(t => t.TopicArn.EndsWith(topicName));
                    if (foundTopic != null)
                    {
                        Console.WriteLine($"Tópico SNS '{topicName}' encontrado.");
                        return foundTopic.TopicArn;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Erro ao listar tópicos (tentativa {i + 1}/{maxRetries}): {ex.Message}");
                }

                Console.WriteLine($"Tópico SNS '{topicName}' não encontrado. Tentando novamente em {delay.TotalSeconds} segundos...");
                await Task.Delay(delay);
            }

            return null;
        }
    }
}