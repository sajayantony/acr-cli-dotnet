using System.CommandLine;
using System.CommandLine.Invocation;
using AzureContainerRegistry.CLI.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;


namespace AzureContainerRegistry.CLI
{
    class ConfigCommand : Command
    {
        public ConfigCommand() : base("config", "Manifest operations")
        {
            var showCmd = new Command("show");
            showCmd.AddArgument(new Argument<string>("reference"));
            showCmd.Handler = CommandHandler.Create<string, IHost>(async (reference, host) =>
            {

                // var loggerFactory = host.Services.GetRequiredService<ILoggerFactory>();
                // _logger = loggerFactory.CreateLogger(typeof(ConfigCommand));
                var registry = host.Services.GetRequiredService<RegistryService>();
                var img = reference.ToImageReference(registry.LoginServer);
                await registry.ShowConfigAsync(img);
            });

            this.Add(showCmd);
        }
    }
}
