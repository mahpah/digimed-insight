using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.Azure.Management.Media;
using Microsoft.Azure.Management.Media.Models;
using Microsoft.Azure.Storage.Blob;
using Microsoft.IdentityModel.Tokens;

namespace Insights.VideoPackager
{
    public class AzureMediaProcessor
    {
        private readonly IAzureMediaServicesClient _client;
        private readonly AzureMediaProcessorOption _option;

        public async Task<string[]> Process(string filePath)
        {
            var uniqueness = Guid.NewGuid().ToString("N");
            var jobName = $"job-{uniqueness}";
            var outputAssetName = $"output-{uniqueness}";
            var inputAssetName = $"input-{uniqueness}";
            var locatorName = $"locator-{uniqueness}";

            var jobInput = await CreateInput(inputAssetName, filePath);
            await CreateOutput(outputAssetName);
            await GetOrCreateTransformAsync();

            JobOutput[] jobOutputs =
            {
                new JobOutputAsset(outputAssetName),
            };

            var job = await _client.Jobs.CreateAsync(
                _option.ResourceGroupName,
                _option.AccountName,
                _option.TransformName,
                jobName,
                new Job
                {
                    Input = jobInput,
                    Outputs = jobOutputs,
                });

            // please check if job is finished
            await WaitForJobToFinishAsync(jobName);
            await CreateStreamLocator(outputAssetName, locatorName);
            return await CreateStreamingUrls(locatorName);
        }

        public AzureMediaProcessor(IAzureMediaServicesClient client, AzureMediaProcessorOption option)
        {
            _client = client;
            _option = option;
        }

        public async Task<StreamingLocator> CreateStreamLocator(string assetName, string locatorName)
        {
            return await _client.StreamingLocators.CreateAsync(_option.ResourceGroupName, _option.AccountName,
                locatorName, new StreamingLocator()
                {
                    AssetName = assetName,
                    StreamingPolicyName = PredefinedStreamingPolicy.ClearStreamingOnly
                });
        }

        private async Task<Job> WaitForJobToFinishAsync(string jobName)
        {
            const int SleepIntervalMs = 20 * 1000;

            Job job = null;

            do
            {
                job = await _client.Jobs.GetAsync(_option.ResourceGroupName, _option.AccountName, _option.TransformName, jobName);

                Console.WriteLine($"Job is '{job.State}'.");
                for (int i = 0; i < job.Outputs.Count; i++)
                {
                    JobOutput output = job.Outputs[i];
                    Console.Write($"\tJobOutput[{i}] is '{output.State}'.");
                    if (output.State == JobState.Processing)
                    {
                        Console.Write($"  Progress: '{output.Progress}'.");
                    }

                    Console.WriteLine();
                }

                if (job.State != JobState.Finished && job.State != JobState.Error && job.State != JobState.Canceled)
                {
                    await Task.Delay(SleepIntervalMs);
                }
            }
            while (job.State != JobState.Finished && job.State != JobState.Error && job.State != JobState.Canceled);

            return job;
        }

        public async Task<string[]> CreateStreamingUrls(string locatorName)
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

            return paths.StreamingPaths.Select(p =>
            {
                var urlBuilder = new UriBuilder();
                urlBuilder.Scheme = "https";
                urlBuilder.Host = streamingEndpoint.HostName;
                urlBuilder.Path = p.Paths[0];
                return urlBuilder.ToString();
            }).ToArray();
        }

        private async Task<JobInputAsset> CreateInput(string assetName, string fileToUpload)
        {
            await _client.Assets.CreateOrUpdateAsync(_option.ResourceGroupName, _option.AccountName, assetName, new Asset());
            var response = await _client.Assets.ListContainerSasAsync(
                _option.ResourceGroupName,
                _option.AccountName,
                assetName,
                permissions: AssetContainerPermission.ReadWrite,
                expiryTime: DateTime.UtcNow.AddHours(4).ToUniversalTime());
            var sasUri = new Uri(response.AssetContainerSasUrls.First());
            var container = new CloudBlobContainer(sasUri);
            var blob = container.GetBlockBlobReference(Path.GetFileName(fileToUpload));
            await blob.UploadFromFileAsync(fileToUpload);

            return new JobInputAsset(assetName);
        }

        private async Task<Asset> CreateOutput(string assetName)
        {
            var outputAsset = await _client.Assets.GetAsync(_option.ResourceGroupName, _option.AccountName, assetName);
            if (outputAsset != null)
            {
                throw new OutputExistsException();
            }

            var asset = new Asset();
            return await _client.Assets.CreateOrUpdateAsync(_option.ResourceGroupName, _option.AccountName, assetName, asset);
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

        public async Task<string> StreamProtected(string assetName, string locatorName, string contentPolicyName)
        {
            await GetOrCreateContentPolicy(contentPolicyName);

            var locator =
                await _client.StreamingLocators.GetAsync(_option.ResourceGroupName, _option.AccountName, locatorName);

            if (locator == null)
            {
                locator = await _client.StreamingLocators.CreateAsync(_option.ResourceGroupName, _option.AccountName,
                    locatorName, new StreamingLocator()
                    {
                        AssetName = assetName,
                        DefaultContentKeyPolicyName = contentPolicyName,
                        StreamingPolicyName = PredefinedStreamingPolicy.MultiDrmCencStreaming
                    });
            }

            var keyIdentifier = locator.ContentKeys
                .Where(k => k.Type == StreamingLocatorContentKeyType.CommonEncryptionCenc)
                .Select(x => x.Id.ToString())
                .First();

            return keyIdentifier;
        }

        public async Task<string> CreateToken(string policyName, string keyIdentifier)
        {
            var policy = await
                _client.ContentKeyPolicies.GetAsync(_option.ResourceGroupName, _option.AccountName, policyName);
            if (policy == null)
            {
                throw new ArgumentException("Policy is not existed");
            }

            var policyProperties =
                await _client.ContentKeyPolicies.GetPolicyPropertiesWithSecretsAsync(_option.ResourceGroupName,
                    _option.AccountName, policyName);
            if (!(policyProperties.Options[0].Restriction is ContentKeyPolicyTokenRestriction restriction))
            {
                throw new InvalidOperationException("Policy not include a token key");
            }

            if (!(restriction.PrimaryVerificationKey is ContentKeyPolicySymmetricTokenKey signingKey))
            {
                throw new InvalidOperationException("Policy not include a token key");
            }

            var tokenSigningKey = new SymmetricSecurityKey(signingKey.KeyValue);
            var cred = new SigningCredentials(
                tokenSigningKey,
                SecurityAlgorithms.HmacSha256,
                SecurityAlgorithms.Sha256Digest);

            var claims = new Claim[]
            {
                new Claim(ContentKeyPolicyTokenClaim.ContentKeyIdentifierClaim.ClaimType, keyIdentifier)
            };

            // To set a limit on how many times the same token can be used to request a key or a license.
            // add  the "urn:microsoft:azure:mediaservices:maxuses" claim.
            // For example, claims.Add(new Claim("urn:microsoft:azure:mediaservices:maxuses", 4));

            var token = new JwtSecurityToken(
                issuer: _option.Issuer,
                audience: _option.Audience,
                claims: claims,
                notBefore: DateTime.Now.AddMinutes(-5),
                expires: DateTime.Now.AddMinutes(60),
                signingCredentials: cred);

            var handler = new JwtSecurityTokenHandler();
            return handler.WriteToken(token);
        }

        private async Task<byte[]> GetOrCreateContentPolicy(string policyName)
        {
            var policy = await
                _client.ContentKeyPolicies.GetAsync(_option.ResourceGroupName, _option.AccountName, policyName);
            if (policy == null)
            {
                var primaryKey = new ContentKeyPolicySymmetricTokenKey(_option.TokenKey);
                var requiredClaims = new List<ContentKeyPolicyTokenClaim>()
                {
                    ContentKeyPolicyTokenClaim.ContentKeyIdentifierClaim
                };

                var restriction = new ContentKeyPolicyTokenRestriction(_option.Issuer, _option.Audience, primaryKey, ContentKeyPolicyRestrictionTokenType.Jwt, null, requiredClaims);

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
                    }
                };

                await _client.ContentKeyPolicies.CreateOrUpdateAsync(_option.ResourceGroupName, _option.AccountName,
                    policyName, options);
            }

            var policyProperties =
                await _client.ContentKeyPolicies.GetPolicyPropertiesWithSecretsAsync(_option.ResourceGroupName,
                    _option.AccountName, policyName);

            if (policyProperties.Options[0].Restriction is ContentKeyPolicyTokenRestriction restriction2)
            {
                if (restriction2.PrimaryVerificationKey is ContentKeyPolicySymmetricTokenKey signingKey)
                {
                   return signingKey.KeyValue;
                }
            }

            return _option.TokenKey;
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

            ContentKeyPolicyPlayReadyConfiguration objContentKeyPolicyPlayReadyConfiguration = new ContentKeyPolicyPlayReadyConfiguration
            {
                Licenses = new List<ContentKeyPolicyPlayReadyLicense> { objContentKeyPolicyPlayReadyLicense }
            };

            return objContentKeyPolicyPlayReadyConfiguration;
        }

        private static ContentKeyPolicyWidevineConfiguration ConfigureWidevineLicenseTemplate()
        {
            WidevineTemplate template = new WidevineTemplate()
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

            ContentKeyPolicyWidevineConfiguration objContentKeyPolicyWidevineConfiguration = new ContentKeyPolicyWidevineConfiguration
            {
                WidevineTemplate = Newtonsoft.Json.JsonConvert.SerializeObject(template)
            };
            return objContentKeyPolicyWidevineConfiguration;
        }
    }

    internal class StreamingEndpointMissingException : Exception
    {
    }

    internal class OutputExistsException : Exception
    {
    }
}
