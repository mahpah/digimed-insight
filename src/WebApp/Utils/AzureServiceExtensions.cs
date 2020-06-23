using System;
using FluentValidation;
using Insights.VideoPackager.Azure;
using Microsoft.Azure.Management.Media;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.Rest.Azure.Authentication;

namespace WebApp.Utils
{
    public static class AzureServiceExtensions
    {
        public static void AddAzureVideoService(this IServiceCollection services)
        {
            services.AddScoped(sp =>
            {
                var configuration = sp.GetConfigure<AzureVideoServiceOption>("AzureVideoServiceOption");

                new AzureVideoServiceOptionValidator().ValidateAndThrow(configuration);

                var credential =
                    ApplicationTokenProvider.LoginSilentAsync(configuration.Credential.TenantId,
                            new ClientCredential(configuration.Credential.ClientId, configuration.Credential.Secret),
                            ActiveDirectoryServiceSettings.Azure)
                        .GetAwaiter()
                        .GetResult();

                var client = new AzureMediaServicesClient(new Uri(configuration.Credential.ArmEndpoint), credential)
                {
                    SubscriptionId = "c104684d-4609-4677-b322-c8e66e49c5f1"
                };

                return new AzureVideoService(client, configuration);
            });
        }

        internal static T GetConfigure<T>(this IServiceProvider serviceProvider, string sectionName = null)
        {
            var configuration = serviceProvider.GetRequiredService<IConfiguration>();
            return string.IsNullOrEmpty(sectionName)
                ? configuration.Get<T>()
                : configuration.GetSection(sectionName).Get<T>();
        }

        internal class AzureVideoServiceOptionValidator : AbstractValidator<AzureVideoServiceOption>
        {
            public AzureVideoServiceOptionValidator()
            {
                RuleFor(x => x.Credential).NotNull();
                RuleFor(x => x.ContentPolicyOption).NotNull();
                RuleFor(x => x.Credential.ArmEndpoint).NotEmpty();
                RuleFor(x => x.Credential.ClientId).NotEmpty();
                RuleFor(x => x.Credential.Secret).NotEmpty();
                RuleFor(x => x.Credential.TenantId).NotEmpty();
                RuleFor(x => x.ResourceGroupName).NotEmpty();
                RuleFor(x => x.AccountName).NotEmpty();
                RuleFor(x => x.TransformName).NotEmpty();
                RuleFor(x => x.StreamingEndpointName).NotEmpty();
                RuleFor(x => x.ContentPolicyOption.ContentPolicyName).NotEmpty();
                RuleFor(x => x.ContentPolicyOption.TokenIssuer).NotEmpty();
                RuleFor(x => x.ContentPolicyOption.TokenAudience).NotEmpty();
                RuleFor(x => x.ContentPolicyOption.TokenValidateKey)
                    .NotEmpty()
                    .MinimumLength(16);


            }
        }
    }
}
