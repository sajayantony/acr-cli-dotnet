using System.CommandLine;
using System.CommandLine.Invocation;
using AzureContainerRegistry.CLI.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;


namespace AzureContainerRegistry.CLI.Commands
{
    class PushCommand : Command
    {
        public PushCommand() : base("push", "Push an artifact")
        {
            this.AddArgument(new Argument<string>("reference"));
            this.Add(new Option<string>("--location",
                    description:  "File path of the contents to upload which containts manifest.json and config.json and other tar files."));

           this.Handler = CommandHandler.Create<string, string, IHost>(async (reference, location, host) =>
           {
               var contentStore = host.Services.GetRequiredService<ContentStore>();
               await contentStore.PushAsync(reference.ToArtifactReference(), location);
           });
        }
    }
}
