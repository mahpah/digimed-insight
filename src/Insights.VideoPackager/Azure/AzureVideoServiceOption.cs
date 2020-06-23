using Microsoft.Rest;

namespace Insights.VideoPackager.Azure
{
    public class AzureVideoServiceOption
    {
        public string ResourceGroupName { get; set; }
        public string AccountName { get; set; }
        public AzureVideoServiceCredential Credential { get; set; }
        public string TransformName { get; set; }
        public ContentPolicyOption ContentPolicyOption { get; set; }
        public string StreamingEndpointName { get; set; } = "default";
    }

    public class AzureVideoServiceCredential
    {
        public string TenantId { get; set; }
        public string ClientId { get; set; }
        public string Secret { get; set; }
        public string ArmEndpoint { get; set; } = "https://management.azure.com";
    }

    public class ContentPolicyOption
    {
        public string ContentPolicyName { get; set; }
        public string TokenValidateKey { get; set; }
        public string TokenIssuer { get; set; }
        public string TokenAudience { get; set; }
    }
}
