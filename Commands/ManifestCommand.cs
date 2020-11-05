using System.CommandLine;
using System.CommandLine.Invocation;
using AzureContainerRegistry.CLI.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;


namespace AzureContainerRegistry.CLI.Commands
{
    class ManifestCommand : Command
    {
        public ManifestCommand() : base("manifest", "Manifest operations")
        {
            var showCmd = new Command("show");
            showCmd.AddOption(new Option<bool>(
                aliases: new string[] { "-raw", "-r" },
                getDefaultValue: () => false,
                "Output the data without formatting"
            ));

            showCmd.AddArgument(new Argument<string>("reference"));
            showCmd.Handler = CommandHandler.Create<string, bool, IHost>(async (reference, raw, host) =>
            {
                host.Services.GetRequiredService<CredentialsProvider>().TryInitialize(reference);
                var registry = host.Services.GetRequiredService<RegistryService>();
                var img = reference.ToArtifactReference();
                await registry.ShowManifestAsync(img, raw);
            });

            this.Add(showCmd);
            this.Add(new ConfigCommand());
        }
    }
}
