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
    class TagCommand : Command
    {
        private ILogger _logger;

        public TagCommand() : base("tag", "Manifest operations")
        {
            var addCmd = new Command("add");
            addCmd.AddArgument(new Argument<string>("source"));
            addCmd.AddArgument(new Argument<string>("target"));
            addCmd.Handler = CommandHandler.Create<string, string, IHost>((source, target, host) =>
             {

                 var loggerFactory = host.Services.GetRequiredService<ILoggerFactory>();
                 _logger = loggerFactory.CreateLogger(typeof(ManifestCommand));
                 var registry = host.Services.GetRequiredService<RegistryService>();
                 return AddTag(registry, source, target);
             });


            this.Add(addCmd);
        }

        async Task<int> AddTag(RegistryService reg, string srcRef, string destRef)
        {
            var src = srcRef.ToImageReference(reg.LoginServer);
            var dest = destRef.ToImageReference(reg.LoginServer);
            await reg.AddTagAsync(src, dest);
            return 0;
        }
    }
}