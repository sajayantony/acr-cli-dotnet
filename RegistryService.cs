using System.Threading.Tasks;
using Microsoft.Azure.ContainerRegistry;
using Microsoft.Azure.ContainerRegistry.Models;
using System;
using OCI;
using System.Text.Json;

namespace AzureContainerRegistry.CLI
{
    class Registry
    {
        private ContainerRegistryCredentials _creds;

        public Registry(string loginUrl, string username, string password)
        {
            if (string.IsNullOrEmpty(loginUrl))
                throw new ArgumentNullException(nameof(loginUrl));

            if (string.IsNullOrEmpty(username))
                throw new ArgumentNullException(nameof(username));

            if (string.IsNullOrEmpty(password))
                throw new ArgumentNullException(nameof(password));


            _creds = new ContainerRegistryCredentials(ContainerRegistryCredentials.LoginMode.TokenAuth, loginUrl, username, password);
        }

        public async Task<Repositories> ListRespositoriesAsync()
        {
            AzureContainerRegistryClient runtimeClient = new AzureContainerRegistryClient(_creds);
            return await runtimeClient.Repository.GetListAsync();
        }

        public async Task ShowManifestV2Async(ImageReference reference)
        {
            if (!String.IsNullOrEmpty(reference.Tag))
            {
                AzureContainerRegistryClient runtimeClient = new AzureContainerRegistryClient(_creds);
                // var manifestResponse = await runtimeClient.Manifests.GetAsync(reference.Repository, reference.Tag, ManifestMediaTypes.ManifestList );
                
                var tagAttrsResponse = await runtimeClient.Tag.GetAttributesAsync(reference.Repository, reference.Tag);                
                var manifestAttrsResponse= await runtimeClient.Manifests.GetAttributesAsync(reference.Repository, tagAttrsResponse.Attributes.Digest);
                var manifestResponse = await runtimeClient.Manifests.GetAsync(reference.Repository, manifestAttrsResponse.Attributes.Digest);
                
                Console.WriteLine(
                    JsonSerializer.Serialize(
                            manifestResponse.Convert(manifestAttrsResponse.Attributes.MediaType), 
                            new JsonSerializerOptions() { 
                                WriteIndented = true, 
                                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                                }).ToString());

            }
        }

        public async Task<TagList> ListTagsAsync(string repo)
        {
            AzureContainerRegistryClient runtimeClient = new AzureContainerRegistryClient(_creds);
            return await runtimeClient.Tag.GetListAsync(repo);
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
        public static object Convert(this ManifestWrapper manifestResponse, string mediaType)
        {
            object manifest = null;
             switch(mediaType)
                {
                    case ManifestMediaTypes.V2Manifest: 
                        manifest = (V2Manifest) manifestResponse;
                        break;
                    case ManifestMediaTypes.V1Manifest: 
                        manifest = (V1Manifest) manifestResponse;
                        break;
                    case ManifestMediaTypes.ManifestList: 
                        manifest = (ManifestList) manifestResponse;
                        break;
                    case ManifestMediaTypes.OCIIndex: 
                        manifest = (OCIIndex) manifestResponse;
                        break;
                    case ManifestMediaTypes.OCIManifest: 
                        manifest = (OCIManifest) manifestResponse;
                        break;
                }

                return manifest;
        }
    }
}


