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
using System.Collections.Generic;

namespace AzureContainerRegistry.CLI.Services
{
    class ContentStore
    {
        private readonly ILogger _logger;
        private readonly TextWriter _output;
        private readonly RegistryService _registry;

        public ContentStore(RegistryService registry, ILoggerFactory loggerFactory, TextWriter output)
        {
            _registry = registry;
            _logger = loggerFactory.CreateLogger(typeof(ContentStore));
            _output = output;
        }

        public static async Task PullAsync(string registry, string repo, string tag)
        {
            var image = new ArtifactReference()
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

        internal async Task<bool> PushAsync(ArtifactReference reference, string dir)
        {
            if (!Directory.Exists(dir))
            {
                throw new Exception($"{dir} not found");
            }

            var manifestFilePath = Path.Join(dir, "manifest.json");
            var configFilePath = Path.Join(dir, "config.json");

            if (!File.Exists(manifestFilePath))
            {
                _logger.LogInformation($"File not found: {manifestFilePath}");
                return false;
            }

            _logger.LogInformation($"Reading {manifestFilePath}");
            V2Manifest? manifest = null;
            using (var fs = File.OpenRead(manifestFilePath))
            using (var txt = new StreamReader(fs))
            {
                JsonSerializer serializer = new JsonSerializer();
                manifest = serializer.Deserialize(txt, typeof(V2Manifest)) as V2Manifest;
            }

            if (manifest == null)
            {
                _logger.LogWarning($"Malformed manifest {manifestFilePath}");
                return false;
            }

            if (!File.Exists(configFilePath!))
            {
                _logger.LogInformation($"File not found: {configFilePath}");
                return false;
            }


            _logger.LogInformation($"Reading {configFilePath}");
            var configFile = new FileInfo(configFilePath).ToDescriptor();
            using (var fs = File.OpenRead(configFilePath))
            {
                _logger.LogInformation($"Uploading Config Blob: {configFilePath} {configFile.Digest}");
                await _registry.UploadBlobAsync(reference, configFile.Digest, fs);
            }

            // Uplaod each layer.
            foreach (var file in Directory.GetFiles(dir))
            {
                _logger.LogInformation($"Uploading Layer {file}");
                var descriptor = new FileInfo(file).ToDescriptor();
                using (var fs = File.OpenRead(file))
                {
                    await _registry.UploadBlobAsync(reference, descriptor.Digest, fs);
                }
            }

            //Put manifest
            _logger.LogInformation("Uploading Manifest");
            await _registry.PutManifestAsync(reference, manifest);

            return true;
        }


        internal async Task<bool> PushAsync(ArtifactReference reference, string configMediaType, string filename)
        {
            Descriptor config = new Descriptor()
            {
                MediaType = !string.IsNullOrEmpty(configMediaType)? configMediaType : "application/vnd.docker.container.image.v1+json"
            };

            var configStream = config.ToStream();
            config.Size = configStream.Length;
            config.Digest = configStream.ComputeHash();
            _logger.LogInformation($"Uploading Config {config.Digest}");
            await _registry.UploadBlobAsync(reference, config.Digest, configStream);

            _logger.LogInformation($"Starting Upload {filename}");
            var blobDescriptor = new FileInfo(filename).ToDescriptor();
            using (var fs = File.OpenRead(filename))
            {
                _logger.LogInformation($"Uploading {filename} with digest {blobDescriptor.Digest}");
                await _registry.UploadBlobAsync(reference, blobDescriptor.Digest, fs);
            }

            V2Manifest manifest = new V2Manifest(2, config: config);
            manifest.Layers = new List<Descriptor>();
            manifest.Layers.Add(blobDescriptor);
            manifest.Config = config;
            //manifest.MediaType = ManifestMediaTypes.OCIManifest;
            await _registry.PutManifestAsync(reference, manifest);
            
            return false;
        }

        public async Task PullAsync(ArtifactReference reference, string outputDir)
        {
            var manifestWithAttributes = await _registry.GetManifestAsync(reference);
            var manifest = manifestWithAttributes.Item1;
            var digest = manifestWithAttributes.Item2?.Digest ?? string.Empty;

            _logger.LogInformation($"Downloading layers for {reference.HostName}/{reference.Repository}@{digest} to {outputDir}");


            if (manifestWithAttributes.Item2 == null)
            {
                throw new ArgumentException("manifestAttributes");
            }

            await DownloadContentsAsync(
                    reference,
                    manifestWithAttributes.Item1,
                    manifestWithAttributes.Item2,
                    outputDir);
        }

        async Task DownloadContentsAsync(ArtifactReference reference, Manifest manifest, ManifestAttributesBase attributes, string outputDir)
        {
            EnsureDirectory(outputDir);

            // Download manifest
            var manifestFile = System.IO.Path.Combine(outputDir, "manifest.json");
            _logger.LogInformation($"Writing Manifest {manifestFile}");

            using (var fs = File.OpenWrite(manifestFile))
            using (var txt = new StreamWriter(fs))
            {
                JsonSerializer serializer = new JsonSerializer();
                serializer.Serialize(txt, manifest);
            }
            _output.WriteLine($"Downloaded manifest.json : {attributes.Digest}");

            // Download config
            var configFile = System.IO.Path.Combine(outputDir, "config.json");
            _logger.LogInformation($"Writing config {configFile}");
            var config = manifest.Config(attributes.MediaType);
            if (config != null)
            {
                await DownloadLayerAsync(reference.Repository, config.Digest, configFile);
                _output.WriteLine($"Downloaded config.json : {config.Digest}");
            }


            //Write Layers        
            var layers = manifest.Layers(attributes.MediaType);
            _logger.LogInformation($"Downloading {layers.Count} Layers.");
            for (int i = 0; i < layers.Count; i++)
            {
                var layer = layers[0];
                // Trim "sha256:" from the digest
                var fileName = layer.Annotations?.Title ?? TrimSha(layer.Digest);
                fileName = System.IO.Path.Combine(outputDir, fileName);
                _output.WriteLine($"Downloading layer    : {layer.Digest}");
                await DownloadLayerAsync(reference.Repository, layer.Digest, fileName);
                _output.WriteLine($"Downloading complete : {layer.Digest}");
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

        public async Task DownloadLayerAsync(ArtifactReference reference, string filename)
        {
            if (string.IsNullOrEmpty(filename))
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
            if (index > -1)
            {
                return digest.Substring(index + 1);
            }

            return digest;
        }

        class AnonymousToken : ServiceClientCredentials
        {
            public ArtifactReference _image;

            public AnonymousToken(ArtifactReference image)
            {
                _image = image;
            }

            public override void InitializeServiceClient<T>(ServiceClient<T> client)
            {
                base.InitializeServiceClient(client);
            }

            private async Task<string?> GetAccessToken()
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
                return jToken?.access_token;
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
            public string? access_token { get; set; }
        }

    }
}