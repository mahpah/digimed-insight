using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Azure.Management.Media;
using Microsoft.Azure.Management.Media.Models;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Rest.Azure;
using Microsoft.Rest.Azure.OData;

namespace Insights.VideoPackager.Azure
{
    public class AzureVideoService
    {
        private readonly IAzureMediaServicesClient _client;
        private readonly AzureVideoServiceOption _option;

        public AzureVideoService(IAzureMediaServicesClient client, AzureVideoServiceOption option)
        {
            _client = client;
            _option = option;
        }

        public async Task<string> CreateAsset(string fileName)
        {
            var assetName = $"input-{Guid.NewGuid():N}";
            await _client.Assets.CreateOrUpdateAsync(_option.ResourceGroupName, _option.AccountName, assetName, new Asset()
            {
                Description = fileName
            });
            return assetName;
        }

        public async Task<string> GenerateUploadUrl(string assetName)
        {
            var assetContainerSas = await _client.Assets.ListContainerSasAsync(
                _option.ResourceGroupName,
                _option.AccountName,
                assetName,
                permissions: AssetContainerPermission.ReadWrite,
                expiryTime: DateTime.UtcNow.AddHours(4).ToUniversalTime());
            return assetContainerSas.AssetContainerSasUrls.First();
        }

        public async Task DeleteMultipleAssets(string[] assetNames)
        {
            foreach (var assetName in assetNames.Where(x => !string.IsNullOrEmpty(x)))
            {
                await _client.Assets.DeleteAsync(_option.ResourceGroupName, _option.ResourceGroupName, assetName);
            }
        }

        public async Task<string> Process(string assetName)
        {
            var transform = await GetOrCreateTransformAsync();
            var name = new Regex("^input-").Replace(assetName, string.Empty);
            var outputName = $"output-{name}";
            var jobName = $"job-{assetName}";

            await CreateOutputAsset(outputName);

            var job = await _client.Jobs.CreateAsync(
                _option.ResourceGroupName,
                _option.AccountName,
                transform.Name,
                jobName,
                new Job
                {
                    Input = new JobInputAsset(assetName),
                    Outputs = new List<JobOutput>()
                    {
                        new JobOutputAsset(outputName)
                        {
                            Label = "output"
                        }
                    },
                });

            return job.Name;
        }

        public async Task<IPage<Job>> ListJobs(string nextToken = null)
        {
            var query = new ODataQuery<Job>();

            return string.IsNullOrEmpty(nextToken)
                ? await _client.Jobs.ListAsync(_option.ResourceGroupName, _option.AccountName, _option.TransformName,
                    query)
                : await _client.Jobs.ListNextAsync(nextToken);
        }

        public async Task<StreamingOutput> GetDrmProtectedStreamingOutput(string assetName)
        {
            var locator = await GetOrCreateDrmProtectedLocator(assetName);
            var manifestPath = await CreateStreamingUrl(locator.Name, StreamingPolicyStreamingProtocol.Dash);

            var locatorContentKey =  locator.ContentKeys
                .Where(k => k.Type == StreamingLocatorContentKeyType.CommonEncryptionCenc)
                .Select(x => x.Id.ToString())
                .First();

            return new StreamingOutput()
            {
                AccessToken = GenerateAccessToken(locatorContentKey),
                ManifestPath = manifestPath
            };
        }

        public async Task<StreamingOutput> GetAesProtectedStreamingOutput(string assetName)
        {
            var locator = await GetOrCreateAesProtectedLocator(assetName);
            var manifestPath = await CreateStreamingUrl(locator.Name, StreamingPolicyStreamingProtocol.Dash);

            var locatorContentKey =  locator.ContentKeys
                .Where(k => k.Type == StreamingLocatorContentKeyType.EnvelopeEncryption)
                .Select(x => x.Id.ToString())
                .First();

            return new StreamingOutput()
            {
                AccessToken = GenerateAccessToken(locatorContentKey),
                ManifestPath = manifestPath
            };
        }

        private string GenerateAccessToken(string locatorContentKey)
        {
            var rawTokenSigningKey = Encoding.ASCII.GetBytes(_option.ContentPolicyOption.TokenValidateKey);
            var tokenSigningKey = new SymmetricSecurityKey(rawTokenSigningKey);
            var cred = new SigningCredentials(
                tokenSigningKey,
                SecurityAlgorithms.HmacSha256,
                SecurityAlgorithms.Sha256Digest);

            var claims = new[]
            {
                new Claim(ContentKeyPolicyTokenClaim.ContentKeyIdentifierClaim.ClaimType, locatorContentKey)
            };

            var token = new JwtSecurityToken(
                issuer: _option.ContentPolicyOption.TokenIssuer,
                audience: _option.ContentPolicyOption.TokenAudience,
                claims: claims,
                notBefore: DateTime.Now.AddMinutes(-5),
                expires: DateTime.Now.AddMinutes(60),
                signingCredentials: cred);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public async Task CreateContentPolicyIfNotExists()
        {
            var policyName = _option.ContentPolicyOption.ContentPolicyName;
            var tokenKey = Encoding.ASCII.GetBytes(_option.ContentPolicyOption.TokenValidateKey);

            var policy = await
                _client.ContentKeyPolicies.GetAsync(_option.ResourceGroupName, _option.AccountName, policyName);
            if (policy == null)
            {
                var primaryKey = new ContentKeyPolicySymmetricTokenKey(tokenKey);
                var requiredClaims = new List<ContentKeyPolicyTokenClaim>()
                {
                    ContentKeyPolicyTokenClaim.ContentKeyIdentifierClaim
                };

                var restriction = new ContentKeyPolicyTokenRestriction(_option.ContentPolicyOption.TokenIssuer,
                    _option.ContentPolicyOption.TokenAudience, primaryKey, ContentKeyPolicyRestrictionTokenType.Jwt, null,
                    requiredClaims);

                var options = new List<ContentKeyPolicyOption>()
                {
                    new ContentKeyPolicyOption()
                    {
                        Name = "playReady",
                        Configuration = ConfigurePlayReadyLicenseTemplate(),
                        Restriction = restriction
                    },
                    new ContentKeyPolicyOption()
                    {
                        Name = "winevine",
                        Configuration = ConfigureWidevineLicenseTemplate(),
                        Restriction = restriction
                    },
                    new ContentKeyPolicyOption(new ContentKeyPolicyClearKeyConfiguration(), restriction)
                    {
                        Name = "AES"
                    }
                };

                await _client.ContentKeyPolicies.CreateOrUpdateAsync(_option.ResourceGroupName, _option.AccountName,
                    policyName, options);
            }
        }

        private async Task CreateOutputAsset(string assetName)
        {
            var outputAsset = await _client.Assets.GetAsync(_option.ResourceGroupName, _option.AccountName, assetName);
            if (outputAsset != null)
            {
                throw new OutputExistsException();
            }

            var asset = new Asset();
            await _client.Assets.CreateOrUpdateAsync(_option.ResourceGroupName, _option.AccountName, assetName, asset);
        }

        // private async Task<byte[]> GetTokenSigningKey()
        // {
        //     var policyProperties =
        //         await _client.ContentKeyPolicies.GetPolicyPropertiesWithSecretsAsync(_option.ResourceGroupName,
        //             _option.AccountName, _option.ContentPolicyOption.ContentPolicyName);
        //     if (!(policyProperties.Options[0].Restriction is ContentKeyPolicyTokenRestriction restriction))
        //     {
        //         throw new InvalidOperationException("Policy not include a token key");
        //     }
        //
        //     if (!(restriction.PrimaryVerificationKey is ContentKeyPolicySymmetricTokenKey signingKey))
        //     {
        //         throw new InvalidOperationException("Policy not include a token key");
        //     }
        //
        //     return signingKey.KeyValue;
        // }

        private async Task<StreamingLocator> GetOrCreateDrmProtectedLocator(string assetName)
        {
            var locatorName = assetName + "-locator";
            var locator =
                await _client.StreamingLocators.GetAsync(_option.ResourceGroupName, _option.AccountName, locatorName);

            if (locator == null)
            {
                locator = await _client.StreamingLocators.CreateAsync(_option.ResourceGroupName, _option.AccountName,
                    locatorName, new StreamingLocator()
                    {
                        AssetName = assetName,
                        DefaultContentKeyPolicyName = _option.ContentPolicyOption.ContentPolicyName,
                        StreamingPolicyName = PredefinedStreamingPolicy.MultiDrmCencStreaming
                    });
            }

            return locator;
        }

        private async Task<StreamingLocator> GetOrCreateAesProtectedLocator(string assetName)
        {
            var locatorName = assetName + "-locator-aes";
            var locator =
                await _client.StreamingLocators.GetAsync(_option.ResourceGroupName, _option.AccountName, locatorName) ??
                await _client.StreamingLocators.CreateAsync(_option.ResourceGroupName, _option.AccountName,
                    locatorName, new StreamingLocator()
                    {
                        AssetName = assetName,
                        DefaultContentKeyPolicyName = _option.ContentPolicyOption.ContentPolicyName,
                        StreamingPolicyName = PredefinedStreamingPolicy.ClearKey
                    });

            return locator;
        }

        private async Task<Transform> GetOrCreateTransformAsync()
        {
            var transform =
                await _client.Transforms.GetAsync(_option.ResourceGroupName, _option.AccountName, _option.TransformName);

            if (transform == null)
            {
                var output = new []
                {
                    new TransformOutput
                    {
                        Preset = new BuiltInStandardEncoderPreset()
                        {
                            PresetName = EncoderNamedPreset.AdaptiveStreaming
                        }
                    }
                };

                // Create the Transform with the output defined above
                transform = await _client.Transforms.CreateOrUpdateAsync(_option.ResourceGroupName, _option.AccountName,
                    _option.TransformName, output);
            }

            return transform;
        }

        private static ContentKeyPolicyPlayReadyConfiguration ConfigurePlayReadyLicenseTemplate()
        {
            var objContentKeyPolicyPlayReadyLicense = new ContentKeyPolicyPlayReadyLicense
            {
                AllowTestDevices = true,
                BeginDate = new DateTime(2016, 1, 1),
                ContentKeyLocation = new ContentKeyPolicyPlayReadyContentEncryptionKeyFromHeader(),
                ContentType = ContentKeyPolicyPlayReadyContentType.UltraVioletStreaming,
                LicenseType = ContentKeyPolicyPlayReadyLicenseType.Persistent,
                PlayRight = new ContentKeyPolicyPlayReadyPlayRight
                {
                    ImageConstraintForAnalogComponentVideoRestriction = true,
                    ExplicitAnalogTelevisionOutputRestriction = new ContentKeyPolicyPlayReadyExplicitAnalogTelevisionRestriction(true, 2),
                    AllowPassingVideoContentToUnknownOutput = ContentKeyPolicyPlayReadyUnknownOutputPassingOption.Allowed
                }
            };

            var objContentKeyPolicyPlayReadyConfiguration = new ContentKeyPolicyPlayReadyConfiguration
            {
                Licenses = new List<ContentKeyPolicyPlayReadyLicense> { objContentKeyPolicyPlayReadyLicense }
            };

            return objContentKeyPolicyPlayReadyConfiguration;
        }

        private static ContentKeyPolicyWidevineConfiguration ConfigureWidevineLicenseTemplate()
        {
            var template = new WidevineTemplate()
            {
                AllowedTrackTypes = "SD_HD",
                ContentKeySpecs = new ContentKeySpec[]
                {
                    new ContentKeySpec()
                    {
                        TrackType = "SD",
                        SecurityLevel = 1,
                        RequiredOutputProtection = new OutputProtection()
                        {
                            HDCP = "HDCP_NONE"
                        }
                    }
                },
                PolicyOverrides = new PolicyOverrides()
                {
                    CanPlay = true,
                    CanPersist = true,
                    CanRenew = false,
                    RentalDurationSeconds = 2592000,
                    PlaybackDurationSeconds = 10800,
                    LicenseDurationSeconds = 604800,
                }
            };

            var objContentKeyPolicyWidevineConfiguration = new ContentKeyPolicyWidevineConfiguration
            {
                WidevineTemplate = Newtonsoft.Json.JsonConvert.SerializeObject(template)
            };
            return objContentKeyPolicyWidevineConfiguration;
        }

        private async Task<string> CreateStreamingUrl(string locatorName,
            StreamingPolicyStreamingProtocol protocol)
        {
            var streamingEndpoint = await _client.StreamingEndpoints.GetAsync(_option.ResourceGroupName, _option.AccountName,
                _option.StreamingEndpointName);

            if (streamingEndpoint == null)
            {
                throw new StreamingEndpointMissingException();
            }

            if (streamingEndpoint.ResourceState != StreamingEndpointResourceState.Running)
            {
                await _client.StreamingEndpoints.StartAsync(_option.ResourceGroupName, _option.AccountName,
                    _option.StreamingEndpointName);
            }

            var paths = await _client.StreamingLocators.ListPathsAsync(_option.ResourceGroupName, _option.AccountName,
                locatorName);

            return paths.StreamingPaths
                .Where(x => x.StreamingProtocol == protocol)
                .Select(p =>
                {
                    var urlBuilder = new UriBuilder
                    {
                        Scheme = "https", Host = streamingEndpoint.HostName, Path = p.Paths[0]
                    };
                    return urlBuilder.ToString();
                })
                .FirstOrDefault();
        }
    }
}
