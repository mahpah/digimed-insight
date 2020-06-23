using Insights.VideoPackager.Azure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace WebApp.Utils
{
    public static class InitializationHelper
    {
        public static IHost Init(this IHost host)
        {
            using var scope = host.Services.CreateScope();
            var azureVideoService = scope.ServiceProvider.GetRequiredService<AzureVideoService>();
            azureVideoService.CreateContentPolicyIfNotExists();

            return host;
        }
    }
}
