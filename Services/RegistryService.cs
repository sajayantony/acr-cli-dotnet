using System.Threading.Tasks;
using Microsoft.Azure.ContainerRegistry;
using Microsoft.Azure.ContainerRegistry.Models;
using System;
using OCI;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.IO;
using System.Text;
using System.Collections.Generic;

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

        public Registry(string registry, string username, string password, TextWriter output)
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

        private TextWriter _output;

        public RegistryService(Registry registry, ILoggerFactory loggerFactory, TextWriter output)
        {
            _logger = loggerFactory.CreateLogger(typeof(RegistryService));
            _registry = registry;
            _output = output;
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

        public async Task ShowManifestV2Async(ImageReference reference, bool raw)
        {

            var manifestWithAttributes = await GetManifestAsync(reference);
            JsonSerializer serializer = new JsonSerializer();
            if (!raw)
            {
                serializer.Formatting = Formatting.Indented;
            }
            serializer.Serialize(_output, manifestWithAttributes.Item1);
        }

        public async Task<(Manifest, ManifestAttributesBase)> GetManifestAsync(ImageReference reference)
        {

            string digest = reference.Digest;
            string manifestMediaType = null;

            // Get the digest pointed to by the given tag
            if (!String.IsNullOrEmpty(reference.Tag))
            {
                //_runtimeClient = new AzureContainerRegistryClient(_creds);
                // var manifestResponse = await runtimeClient.Manifests.GetAsync(reference.Repository, reference.Tag, ManifestMediaTypes.ManifestList );

                _logger.LogInformation($"GET Tag Attributes {reference.HostName}/{reference.Repository}:{reference.Tag}");
                var tagAttrsResponse = await _runtimeClient.Tag.GetAttributesAsync(reference.Repository, reference.Tag);
                digest = tagAttrsResponse.Attributes.Digest;
            }

            _logger.LogInformation($"GET Manifest Attributes: {reference.HostName}/{reference.Repository}@{digest}");
            var manifestAttrsResponse = await _runtimeClient.Manifests.GetAttributesAsync(reference.Repository, digest);
            manifestMediaType = manifestAttrsResponse.Attributes.MediaType;

            _logger.LogInformation($"GET Manifest {reference.HostName}/{reference.Repository}@{digest} with MediaType: {manifestAttrsResponse.Attributes.MediaType}");
            var manifest = await _runtimeClient.Manifests.GetAsync(reference.Repository, digest, manifestMediaType);
            return (manifest.Convert(manifestAttrsResponse.Attributes.MediaType), manifestAttrsResponse.Attributes);
        }

        internal async Task ShowConfigAsync(ImageReference img, bool raw)
        {
            var manifestWithAttributes = await GetManifestAsync(img);
            var manifest = manifestWithAttributes.Item1;
            switch (manifest)
            {
                case V2Manifest v2m:
                    await WriteBlobAsync(img.Repository, v2m.Config.Digest, v2m.Config.Size, raw);
                    break;

                case OCIManifest oci:
                    await WriteBlobAsync(img.Repository, oci.Config.Digest, oci.Config.Size, raw);
                    break;

                default:
                    _output.Write($"No config present in manifest of type {manifestWithAttributes.Item2.MediaType}");
                    break;
            }
        }

        async Task WriteBlobAsync(string repo, string digest, long? size, bool raw)
        {
            // Ideally validate during download
            if (size.HasValue && size.Value < 10 * 1024 * 1024)
            {
                using (var stream = await this.GetBlobAsync(repo, digest))
                using (MemoryStream mem = new MemoryStream())
                {
                    await stream.CopyToAsync(mem);
                    mem.Position = 0;
                    var json = Encoding.UTF8.GetString(mem.GetBuffer());
                    var o = JsonConvert.DeserializeObject(json);
                    JsonSerializer serializer = new JsonSerializer();
                    if (!raw)
                    {
                        serializer.Formatting = Formatting.Indented;
                    }
                    serializer.Serialize(_output, o);
                }
            }
            else
            {
                _logger.LogCritical($"Size of config {size.Value} exceed max possible value of 10MB");
            }
        }

        public async Task AddTagAsync(ImageReference reference, ImageReference dest)
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
                serializer.Serialize(_output, manifestResponse.Convert(manifestAttrsResponse.Attributes.MediaType));

                _logger.LogInformation($"Putting Tag with Manifest:  {dest.HostName}/{dest.Repository}:{dest.Tag} {manifestAttrsResponse.Attributes.Digest}");
                _runtimeClient = new AzureContainerRegistryClient(_creds);
                await _runtimeClient.Manifests.CreateAsync("hello-world", "test-put", manifestResponse.Convert(manifestAttrsResponse.Attributes.MediaType));
            }
        }

        public async Task<Stream> GetBlobAsync(string repo, string digest)
        {
            return await _runtimeClient.Blob.GetAsync(repo, digest);
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


        public static IList<Descriptor> Layers(this Manifest manifest, string mediaType)
        {
            switch(manifest)
            {
                case OCIManifest oci:
                    return oci.Layers;
                case V2Manifest v2m:
                    return v2m.Layers;
            }

            return new List<Descriptor>();
        }

        public static Descriptor Config(this Manifest manifest, string mediaType)
        {
            switch(manifest)
            {
                case OCIManifest oci:
                    return oci.Config;
                case V2Manifest v2m:
                    return v2m.Config;
            }

            return null;
        }
    }
}


