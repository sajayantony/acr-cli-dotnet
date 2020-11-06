
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Rest;
using OCI;
using AzureContainerRegistry.CLI;
using Microsoft.Azure.ContainerRegistry;

namespace AzureContainerRegistry.CLI.Services
{
    class CredentialsProvider
    {
        private ServiceClientCredentials _creds;

        public ServiceClientCredentials Credentials => _creds;        
        
        public void TryInitialize(string reference)
        {
            if (_creds == null)
            {
                _creds = new AnonymousToken(reference.ToArtifactReference());
            }
        }

        public CredentialsProvider()
        {

        }

        public CredentialsProvider(string registry, string username, string password)
        {
            _creds = new ContainerRegistryCredentials(
                   ContainerRegistryCredentials.LoginMode.Basic,
                   registry,
                   username,
                   password);
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

            class AuthToken
            {
                public string access_token { get; set; }
            }
        }
    }
}
