using System.Threading.Tasks;
using Microsoft.Azure.ContainerRegistry;
using Microsoft.Azure.ContainerRegistry.Models;
using System;
using OCI;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.IO;
using System.Text;

namespace AzureContainerRegistry.CLI.Services
{

    class RegistryService
    {
        private AzureContainerRegistryClient _runtimeClient;

        private ILogger _logger;

        private TextWriter _output;

        public RegistryService(CredentialsProvider credsproviders, ILoggerFactory loggerFactory, TextWriter output)
        {
            _logger = loggerFactory.CreateLogger(typeof(RegistryService));
            _output = output;
            _runtimeClient = new AzureContainerRegistryClient(credsproviders.Credentials);
        }

        public async Task<Repositories> ListRespositoriesAsync()
        {
            return await _runtimeClient.Repository.GetListAsync();
        }

        public async Task ShowManifestAsync(ArtifactReference reference, bool raw)
        {
            var manifestWithAttributes = await GetManifestAsync(reference);
            JsonSerializer serializer = new JsonSerializer();
            if (!raw)
            {
                serializer.Formatting = Formatting.Indented;
            }

            serializer.Serialize(_output, manifestWithAttributes.Item1);
        }

        public async Task<(Manifest, ManifestAttributesBase?)> GetManifestAsync(ArtifactReference reference)
        {
            ManifestAttributes? manifestAttrResp = null;
            string mediaType = OCI.ManifestMediaTypes.V2Manifest;
            var digest = reference.Digest;

            try
            {
                // Get the digest pointed to by the given tag
                if (!String.IsNullOrEmpty(reference.Tag))
                {
                    _logger.LogInformation($"GET Tag Attributes {reference.HostName}/{reference.Repository}:{reference.Tag}");
                    var tagAttrsResponse = await _runtimeClient.Tag.GetAttributesAsync(reference.Repository, reference.Tag);
                    digest = tagAttrsResponse.Attributes.Digest;
                }

                _logger.LogInformation($"GET Manifest Attributes: {reference.HostName}/{reference.Repository}@{digest}");
                manifestAttrResp = await _runtimeClient.Manifests.GetAttributesAsync(reference.Repository, digest);
                _logger.LogInformation($"GET Manifest {reference.HostName}/{reference.Repository}@{digest} with MediaType: {manifestAttrResp.Attributes.MediaType}");

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error when getting Attributes. Might be not an Azure Container Registry or might not have access to metadata API.");
            }

            if (manifestAttrResp != null)
            {
                mediaType = manifestAttrResp?.Attributes.MediaType ?? OCI.ManifestMediaTypes.V2Manifest;
                digest = manifestAttrResp?.Attributes?.Digest ?? string.Empty;
            }


            var manifest = await _runtimeClient.Manifests.GetAsync(reference.Repository, digest, mediaType);
            return (manifest.Convert(mediaType), manifestAttrResp?.Attributes);
        }

        internal async Task ShowConfigAsync(ArtifactReference img, bool raw)
        {
            var manifestWithAttributes = await GetManifestAsync(img);
            var manifest = manifestWithAttributes.Item1;

            var config = manifest.Config(manifestWithAttributes.Item2?.MediaType ?? string.Empty);
            if (config != null && config.Size.HasValue)
            {
                await WriteBlobAsync(img.Repository, config.Digest, config.Size.Value, raw);
                return;
            }

            _output.Write($"No config present in manifest");
        }

        public async Task<bool> UploadBlobAsync(ArtifactReference reference, string digest, Stream blobStream)
        {
            _logger.LogInformation($"Uploading Blob {reference} to {digest}");
            var repository = reference.Repository;
            var uploadInfo = await _runtimeClient.Blob.StartUploadAsync(repository);
            var uploadedLayer = await _runtimeClient.Blob.UploadAsync(blobStream, uploadInfo.Location);
            var uploadedLayerEnd = await _runtimeClient.Blob.EndUploadAsync(digest, uploadedLayer.Location);
            return uploadedLayerEnd.DockerContentDigest == digest;
        }

        public async Task AddTagAsync(ArtifactReference reference, ArtifactReference dest)
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
                await _runtimeClient.Manifests.CreateAsync(dest.Repository, dest.Tag, manifestResponse.Convert(manifestAttrsResponse.Attributes.MediaType));
            }
        }

        public async Task PutManifestAsync(ArtifactReference reference, Manifest manifest)
        {
            // https://github.com/Azure/azure-sdk-for-net/issues/17084
            if(manifest is OCIManifest)
            {
                throw new NotImplementedException(); 
            }

            _logger.LogInformation($"Writing manifest {manifest.GetType().Name} to {reference}");
            var resp = await _runtimeClient.Manifests.CreateAsync(
                reference.Repository,
                reference.Tag,
                manifest);
        }

        public async Task<Stream> GetBlobAsync(string repo, string digest)
        {
            return await _runtimeClient.Blob.GetAsync(repo, digest);
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
                _logger.LogCritical($"Size of config {size} exceed max possible value of 10MB");
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
}


