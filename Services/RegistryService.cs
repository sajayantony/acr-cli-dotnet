using System.Threading.Tasks;
using Microsoft.Azure.ContainerRegistry;
using Microsoft.Azure.ContainerRegistry.Models;
using System;
using OCI;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace AzureContainerRegistry.CLI.Services
{

    class Registry
    {
        private readonly string _registry;
        private readonly string _username;
        private readonly string _password;

        public string LoginUrl => _registry;

        public string UserName => _username;

        public string Password => _password;

        public Registry(string registry, string username, string password)
        {
            if (string.IsNullOrEmpty(registry))
                throw new ArgumentNullException(nameof(registry));

            if (string.IsNullOrEmpty(username))
                throw new ArgumentNullException(nameof(username));

            if (string.IsNullOrEmpty(password))
                throw new ArgumentNullException(nameof(password));

            _registry = registry;
            _username = username;
            _password = password;
        }
    }

    class RegistryService
    {
        private ContainerRegistryCredentials _creds;
        private AzureContainerRegistryClient _runtimeClient;

        public string LoginServer => _registry.LoginUrl;

        private ILogger _logger;
        private Registry _registry;

        public RegistryService(Registry registry, ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger(typeof(RegistryService));
            _registry = registry;
            _creds = new ContainerRegistryCredentials(
                ContainerRegistryCredentials.LoginMode.TokenAuth,
                registry.LoginUrl,
                registry.UserName,
                registry.Password);
            _runtimeClient = new AzureContainerRegistryClient(_creds);
        }

        public async Task<Repositories> ListRespositoriesAsync()
        {
            return await _runtimeClient.Repository.GetListAsync();
        }

        public async Task ShowManifestV2Async(ImageReference reference)
        {

            _logger.LogInformation($"Fetching manifest {reference.HostName}/{reference.Repository}:{reference.Tag}");

            if (!String.IsNullOrEmpty(reference.Tag))
            {
                //_runtimeClient = new AzureContainerRegistryClient(_creds);
                // var manifestResponse = await runtimeClient.Manifests.GetAsync(reference.Repository, reference.Tag, ManifestMediaTypes.ManifestList );

                var tagAttrsResponse = await _runtimeClient.Tag.GetAttributesAsync(reference.Repository, reference.Tag);

                _logger.LogInformation($"Tag digest: {tagAttrsResponse.Attributes.Digest}");

                var manifestAttrsResponse = await _runtimeClient.Manifests.GetAttributesAsync(reference.Repository, tagAttrsResponse.Attributes.Digest);

                _logger.LogInformation($"MediaType: {manifestAttrsResponse.Attributes.MediaType}");
                var manifestResponse = await _runtimeClient.Manifests.GetAsync(reference.Repository, tagAttrsResponse.Attributes.Digest);

                JsonSerializer serializer = new JsonSerializer();
                serializer.Formatting = Formatting.Indented;
                serializer.Serialize(Console.Out, manifestResponse.Convert(manifestAttrsResponse.Attributes.MediaType));
            }
        }

        internal async Task AddTagAsync(ImageReference reference, ImageReference dest)
        {
            _logger.LogInformation($"Fetching manifest {reference.HostName}/{reference.Repository}:{reference.Tag}");

            if (!String.IsNullOrEmpty(reference.Tag))
            {
                var tagAttrsResponse = await _runtimeClient.Tag.GetAttributesAsync(reference.Repository, reference.Tag);

                _logger.LogInformation($"Tag digest: {tagAttrsResponse.Attributes.Digest}");

                var manifestAttrsResponse = await _runtimeClient.Manifests.GetAttributesAsync(reference.Repository, tagAttrsResponse.Attributes.Digest);

                _logger.LogInformation($"MediaType: {manifestAttrsResponse.Attributes.MediaType}");
                var manifestResponse = await _runtimeClient.Manifests.GetAsync(reference.Repository, tagAttrsResponse.Attributes.Digest);

                
                JsonSerializer serializer = new JsonSerializer();
                serializer.Formatting = Formatting.Indented;
                serializer.Serialize(Console.Out, manifestResponse.Convert(manifestAttrsResponse.Attributes.MediaType));

                 _logger.LogInformation($"Putting Tag with Manifest:  {dest.HostName}/{dest.Repository}:{dest.Tag} {manifestAttrsResponse.Attributes.Digest}");
                 _runtimeClient = new AzureContainerRegistryClient(_creds);
                await _runtimeClient.Manifests.CreateAsync("hello-world", "test-put", manifestResponse.Convert(manifestAttrsResponse.Attributes.MediaType));
            }

        }

        public async Task<TagList> ListTagsAsync(string repo)
        {

            return await _runtimeClient.Tag.GetListAsync(repo);
        }
    }

    public class TagListFilter
    {
        public string Digest { get; private set; }

        public TagListFilter(string digest)
        {
            Digest = digest;
        }
    }

    public static class ManifestHelpers
    {
        public static Manifest Convert(this ManifestWrapper manifestResponse, string mediaType)
        {
            Manifest manifest = null;
            switch (mediaType)
            {
                case ManifestMediaTypes.V2Manifest:
                    manifest = (V2Manifest)manifestResponse;
                    break;
                case ManifestMediaTypes.V1Manifest:
                    manifest = (V1Manifest)manifestResponse;
                    break;
                case ManifestMediaTypes.ManifestList:
                    manifest = (ManifestList)manifestResponse;
                    break;
                case ManifestMediaTypes.OCIIndex:
                    manifest = (OCIIndex)manifestResponse;
                    break;
                case ManifestMediaTypes.OCIManifest:
                    manifest = (OCIManifest)manifestResponse;
                    break;
            }

            return manifest;
        }
    }
}


