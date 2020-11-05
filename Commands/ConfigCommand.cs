using System.CommandLine;
using System.CommandLine.Invocation;
using AzureContainerRegistry.CLI.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;


namespace AzureContainerRegistry.CLI.Commands
{
    class ConfigCommand : Command
    {
        public ConfigCommand() : base("config", "Config operations")
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
               var registry = host.Services.GetRequiredService<RegistryService>();
               var img = reference.ToArtifactReference();
               await registry.ShowConfigAsync(img, raw);
           });

            this.Add(showCmd);
        }
    }
}
