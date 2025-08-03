using AWS.Services;
using Microsoft.Extensions.DependencyInjection;

namespace AWS.Configuration
{
    public static class DependencyInjectionConfig
    {
        public static void AddDependencyInjectionConfiguration(this IServiceCollection services)
        {
            services.AddSingleton<ISnsPublisher, SnsPublisher>();
            services.AddSingleton<ISqsConsumer, SqsConsumer>();
        }
    }
}
