using System.CommandLine;
using System.CommandLine.Invocation;
using AzureContainerRegistry.CLI.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;


namespace AzureContainerRegistry.CLI.Commands
{
    class ConfigCommand : Command
    {
        public ConfigCommand() : base("config", "Manifest operations")
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
               // var loggerFactory = host.Services.GetRequiredService<ILoggerFactory>();
               // _logger = loggerFactory.CreateLogger(typeof(ConfigCommand));
               var registry = host.Services.GetRequiredService<RegistryService>();
               var img = reference.ToImageReference(registry.LoginServer);
               await registry.ShowConfigAsync(img, raw);
           });

            this.Add(showCmd);
        }
    }
}
