namespace Insights.VideoPackager
{
    public class AzureMediaProcessorOption
    {
        public string ResourceGroupName { get; set; }
        public string AccountName { get; set; }
        public string TransformName { get; set; }
        public string StreamingEndpointName { get; set; }
        public byte[] TokenKey { get; set; }
        public string Issuer { get; set; }
        public string Audience { get; set; }
    }
}
