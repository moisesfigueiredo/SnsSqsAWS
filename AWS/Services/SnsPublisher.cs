using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;
using AWS.Dto;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace AWS.Services
{
    public class SnsPublisher : ISnsPublisher
    {
        private readonly AmazonSimpleNotificationServiceClient _snsClient;
        private readonly AwsSettings _awsSettings;
        private string _topicArn;

        public SnsPublisher(AmazonSimpleNotificationServiceClient snsClient, IOptions<AwsSettings> awsOptions)
        {
            _snsClient = snsClient;
            _awsSettings = awsOptions.Value;
        }

        public async Task PublishAsync<T>(T message)
        {
            Console.WriteLine("Publicando mensagem no SNS...");

            if (string.IsNullOrEmpty(_topicArn))
            {
                _topicArn = await GetTopicArnWithRetry(_awsSettings.SnsTopicName, 5, TimeSpan.FromSeconds(2));

                if (string.IsNullOrEmpty(_topicArn))
                {
                    Console.WriteLine($"Erro: Tópico SNS '{_awsSettings.SnsTopicName}' não encontrado. A publicação falhou.");
                    return;
                }
            }

            var jsonMessage = JsonSerializer.Serialize(message);

            var publishRequest = new PublishRequest
            {
                TopicArn = _topicArn,
                Message = jsonMessage,
                Subject = "Teste de Mensagem"
            };

            try
            {
                var publishResponse = await _snsClient.PublishAsync(publishRequest);
                Console.WriteLine($"Mensagem publicada com sucesso! ID da mensagem: {publishResponse.MessageId}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao publicar mensagem: {ex.Message}");
            }
        }

        private async Task<string> GetTopicArnWithRetry(string topicName, int maxRetries, TimeSpan delay)
        {
            for (int i = 0; i < maxRetries; i++)
            {
                try
                {
                    var topicResponse = await _snsClient.ListTopicsAsync(new ListTopicsRequest());
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