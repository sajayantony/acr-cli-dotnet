using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Threading.Tasks;
using AzureContainerRegistry.CLI;
using AzureContainerRegistry.CLI.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OCI;

class ManifestCommand : Command
{
    private ILogger _logger;

    public ManifestCommand() : base("manifest", "Manifest operations")
    {
        var showCmd = new Command("show");
        showCmd.AddArgument(new Argument<string>("reference"));        
        showCmd.Handler = CommandHandler.Create<string, IHost>((reference, host) =>
        {

            var loggerFactory = host.Services.GetRequiredService<ILoggerFactory>();
            _logger = loggerFactory.CreateLogger(typeof(ManifestCommand));
            var registry = host.Services.GetRequiredService<Registry>();
            return ShowManifestV2(registry, reference);
        });

        
        this.Add(showCmd);
    }

    async Task<int> ShowManifestV2(Registry reg, string reference)
    {
        _logger.LogInformation("Querying manifest for ....");
        var img = new ImageReference();
        img.HostName = reg.LoginServer;
        
        var hostPrefix  =  reg.LoginServer + "/";
        if(reference.StartsWith(hostPrefix))
        {
            //Trim the registry to get repository and tag. 
            reference = reference.Substring(hostPrefix.Length);            
            if(reference.Contains(':'))
            {
              var parts = reference.Split(':');
              img.Repository = parts[0];
              img.Tag = parts[1];
            }
        }

        _logger.LogInformation($"Getting manifest for {img.HostName}{img.Repository}:{img.Tag}");
        
        await reg.ShowManifestV2Async(img);
     
        return 0;
    }
}