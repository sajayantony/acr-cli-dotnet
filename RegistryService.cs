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
            AzureContainerRegistryClient runtimeClient = new AzureContainerRegistryClient(_creds);            
            var manifestResponse = await runtimeClient.Manifests.GetAsync(reference.Repository, reference.Tag, ManifestMediaTypes.V2Manifest);
            Console.WriteLine(JsonSerializer.Serialize(manifestResponse, new JsonSerializerOptions() { WriteIndented = true }).ToString());
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
}

