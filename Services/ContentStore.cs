using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.ContainerRegistry;
using System.Threading;
using Microsoft.Rest;
using System.Net.Http.Headers;
using System.IO;
using OCI;
using AzureContainerRegistry.CLI.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.ContainerRegistry.Models;
using Newtonsoft.Json;

namespace AzureContainerRegistry.CLI
{
    class ContentStore
    {
        private ILogger _logger;
        private TextWriter _output;
        private RegistryService _registry;

        public ContentStore(RegistryService registry, ILoggerFactory loggerFactory, TextWriter output)
        {
            _registry = registry;
            _logger = loggerFactory.CreateLogger(typeof(ContentStore));
            _output = output;
        }

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
            //Console.WriteLine(JsonSerializer.Serialize(manifestResponse, new JsonSerializerOptions() { WriteIndented = true }).ToString());

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

        public async Task PullAsync(ImageReference reference, string outputDir)
        {
            var manifestWithAttributes = await _registry.GetManifestAsync(reference);
            var manifest = manifestWithAttributes.Item1;
            var digest = manifestWithAttributes.Item2.Digest;

            _logger.LogInformation($"Downloading layers for {reference.HostName}/{reference.Repository}@{digest} to {outputDir}");

            if (manifest is V2Manifest)
            {

                EnsureDirectory(outputDir);
                // Download manifest
                var manifestFile = System.IO.Path.Combine(outputDir, "manifest.json");
                _logger.LogInformation($"Writing Manifest {manifestFile}");

                using (var fs = File.OpenWrite(manifestFile))
                using (var txt = new StreamWriter(fs))
                {
                    JsonSerializer serializer = new JsonSerializer();
                    serializer.Serialize(txt, manifestWithAttributes.Item1);
                }
                _output.WriteLine($"Downloaded manifest.json : {digest}");

                // Download config
                var v2m = manifest as V2Manifest;
                var configFile = System.IO.Path.Combine(outputDir, "config.json");
                _logger.LogInformation($"Writing config {configFile}");
                await DownloadLayerAsync(reference.Repository, v2m.Config.Digest, configFile);
                _output.WriteLine($"Downloaded config.json : {v2m.Config.Digest}");

                //Write Layers               
                _logger.LogInformation($"Downloading {v2m.Layers.Count} Layers.");
                for (int i = 0; i < v2m.Layers.Count; i++)
                {
                    var layer = v2m.Layers[0];
                    // Trim "sha256:" from the digest
                    var fileName = layer.Annotations?.Title ?? TrimSha(digest);
                    fileName = System.IO.Path.Combine(outputDir, fileName);
                    _output.WriteLine($"Downloading layer    : {layer.Digest}");
                    await DownloadLayerAsync(reference.Repository, layer.Digest, fileName);
                    _output.WriteLine($"Downloading complete : {layer.Digest}");
                }
            }
        }

        public async Task DownloadLayerAsync(string repo, string digest, string filename)
        {
            if (String.IsNullOrEmpty(filename))
            {
                throw new ArgumentNullException(nameof(filename));
            }

            using (var blobStream = await _registry.GetBlobAsync(repo, digest))
            {
                _logger.LogInformation($"Writing File: {System.IO.Path.GetFullPath(filename)}");
                using (FileStream fs = File.OpenWrite(filename))
                {
                    await blobStream.CopyToAsync(fs);
                }
            }
        }

        public async Task DownloadLayerAsync(ImageReference reference, string filename)
        {
            if(string.IsNullOrEmpty(filename))
            {
                filename = TrimSha(reference.Digest);
            }
            
            await DownloadLayerAsync(reference.Repository, reference.Digest, filename);
        }


        void EnsureDirectory(string dir)
        {
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
        }

        string TrimSha(string digest)
        {
            int index = digest.IndexOf(':');
            if(index > -1)
            {
                return digest.Substring(index +1);
            }

            return digest;
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
                var jToken = System.Text.Json.JsonSerializer.Deserialize<AuthToken>(strResponse);
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