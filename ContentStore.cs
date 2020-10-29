using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.ContainerRegistry;
using System.Text.Json;
using System.Threading;
using Microsoft.Rest;
using System.Net.Http.Headers;
using System.IO;
using OCI;

namespace AzureContainerRegistry.CLI
{
    public class ContentStore
    {
        public static async Task Pull(string registry, string repo, string tag)
        {
            var image = new ImageReference()
            {
                HostName = registry,
                Repository = repo,
                Tag = tag
            };

            var loginUri = $"https://{registry}";
            AzureContainerRegistryClient runtimeClient = new AzureContainerRegistryClient(registry, new AnonymousToken(image));

            //Get manifest 
            var manifestResponse = runtimeClient.Manifests.GetAsync(repo, tag, "application/vnd.oci.image.manifest.v1+json").Result;
            Console.WriteLine("Manifest:");
            Console.WriteLine(JsonSerializer.Serialize(manifestResponse, new JsonSerializerOptions() { WriteIndented = true }).ToString());

            //Dowload multiple layers here.
            Console.WriteLine("Starting Layer Download.....");
            for (int i = 0; i < manifestResponse.Layers.Count; i++)
            {
                var l0 = manifestResponse.Layers[0];
                var blobStream = await runtimeClient.Blob.GetAsync(repo, l0.Digest);
                var fileName = l0.Annotations.Title;
                Console.WriteLine($"Writing File: {fileName}");
                using (FileStream fs = File.OpenWrite(fileName))
                {
                    await blobStream.CopyToAsync(fs);
                }
            }
        }

        class AnonymousToken : ServiceClientCredentials
        {
            public ImageReference _image;

            public AnonymousToken(ImageReference image)
            {
                _image = image;
            }

            public override void InitializeServiceClient<T>(ServiceClient<T> client)
            {
                base.InitializeServiceClient(client);
            }

            private async Task<string> GetAccessToken()
            {
                HttpClient c = new HttpClient();                

                var service = _image.HostName;
                var repo = _image.Repository;
                var hostname = System.Environment.MachineName;
                var scope = $"repository:{repo}:pull";
                string uri = $"https://{service}/oauth2/token?client={hostname}&scope={scope}&service={service}";

                var response = c.GetAsync(uri).Result;
                var strResponse = await response.Content.ReadAsStringAsync();

                //Console.WriteLine(strResponse);
                var jToken = JsonSerializer.Deserialize<AuthToken>(strResponse);
                return jToken.access_token;
            }

            public override Task ProcessHttpRequestAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                var accessToken = GetAccessToken().Result;
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                return base.ProcessHttpRequestAsync(request, cancellationToken);
            }
        }

        class AuthToken
        {
            public string access_token { get; set; }
        }

    }
}