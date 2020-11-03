using System.CommandLine;
using System.CommandLine.Invocation;
using AzureContainerRegistry.CLI.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;


namespace AzureContainerRegistry.CLI.Commands
{
    class LayerCommand : Command
    {

        public LayerCommand() : base("layer", "Layer Operations")
        {
            var pullCmd = new Command("pull");

            pullCmd.AddArgument(new Argument<string>("reference", "Layer reference e.g. myregistry.azurecr.io/repo@sha25627e17ff3"));
            pullCmd.Add(new Option<string>(
                    aliases: new string[] { "--output", "-o" },                    
                    description: "Filename to download contents. Defaults to digest of the layer"));
            pullCmd.Handler = CommandHandler.Create<string, string, IHost>(async (reference, output, host) =>
           {
               var contentStore = host.Services.GetRequiredService<ContentStore>();
               var registry = host.Services.GetRequiredService<Registry>();
               await contentStore.DownloadLayerAsync(
                   reference.ToImageReference(registry.LoginUrl), 
                   filename: output);
           });

           this.Add(pullCmd);
        }
    }
}
