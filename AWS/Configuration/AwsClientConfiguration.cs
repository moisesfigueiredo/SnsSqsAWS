using Amazon.SimpleNotificationService;
using Amazon.SQS;
using AWS.Dto;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace AWS.Configuration
{
    public static class AwsClientConfiguration
    {
        public static string AwsServiceUrl { get; } = Environment.GetEnvironmentVariable("AWS_SERVICE_URL");
        public static string AwsRegion { get; } = Environment.GetEnvironmentVariable("AWS_REGION");

        public static void RegisterAwsClients(this IServiceCollection services)
        {
            services.AddSingleton<AmazonSimpleNotificationServiceClient>(sp =>
            {
                var awsSettings = sp.GetRequiredService<IOptions<AwsSettings>>().Value;

                return new AmazonSimpleNotificationServiceClient(new AmazonSimpleNotificationServiceConfig
                {
                    ServiceURL = AwsServiceUrl ?? awsSettings.ServiceURL,
                    AuthenticationRegion = AwsRegion ?? awsSettings.Region
                });
            });

            services.AddSingleton<AmazonSQSClient>(sp =>
            {
                var awsSettings = sp.GetRequiredService<IOptions<AwsSettings>>().Value;

                return new AmazonSQSClient(new AmazonSQSConfig
                {
                    ServiceURL = AwsServiceUrl ?? awsSettings.ServiceURL,
                    AuthenticationRegion = AwsRegion ?? awsSettings.Region
                });
            });
        }
    }
}
