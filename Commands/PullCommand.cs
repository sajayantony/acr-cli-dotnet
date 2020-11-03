using System.CommandLine;
using System.CommandLine.Invocation;
using AzureContainerRegistry.CLI.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;


namespace AzureContainerRegistry.CLI
{
    class PullCommand : Command
    {

        public PullCommand() : base("pull", "Pull Artifact Operations")
        {
            this.AddArgument(new Argument<string>("reference"));
            this.Add(new Option<string>(
                    aliases: new string[] { "--output", "-o" },
                    getDefaultValue: () => System.IO.Directory.GetCurrentDirectory(),
                    description: "Output Directory to download contents"));

            this.Handler = CommandHandler.Create<string, string,  IHost>(async (reference, output,  host) =>
            {
                var contentStore = host.Services.GetRequiredService<ContentStore>();
                var registry = host.Services.GetRequiredService<Registry>();
                await contentStore.PullAsync(reference.ToImageReference(registry.LoginUrl) , output);
            });
        }
    }
}
