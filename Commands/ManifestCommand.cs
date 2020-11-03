using System.CommandLine;
using System.CommandLine.Invocation;
using System.Threading.Tasks;
using AzureContainerRegistry.CLI.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OCI;


namespace AzureContainerRegistry.CLI
{
    class ManifestCommand : Command
    {

        public ManifestCommand() : base("manifest", "Manifest operations")
        {
            var showCmd = new Command("show");
            showCmd.AddArgument(new Argument<string>("reference"));
            showCmd.Handler = CommandHandler.Create<string, IHost>(async (reference, host) =>
            {

                // var loggerFactory = host.Services.GetRequiredService<ILoggerFactory>();
                // _logger = loggerFactory.CreateLogger(typeof(ManifestCommand));
                var registry = host.Services.GetRequiredService<RegistryService>();
                var img = reference.ToImageReference(registry.LoginServer);
                await registry.ShowManifestV2Async(img);
            });


            this.Add(showCmd);
        }
    }
}
