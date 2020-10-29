using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Threading.Tasks;
using AzureContainerRegistry.CLI;
using OCI;

class ManifestCommand : Command
{
    public ManifestCommand(CommandRegistryContext ctx) : base("manifest", "Manifest operations")
    {
        var showCmd = new Command("show");
        showCmd.AddArgument(new Argument<string>("reference"));        
        showCmd.Handler = CommandHandler.Create<string>((reference) =>
        {
            return ShowManifestV2(ctx.Registry, ctx.Username, ctx.Password, reference);
        });

        
        this.Add(showCmd);
    }

    static async Task<int> ShowManifestV2(string registry, string username, string password, string reference)
    {
        var reg = new Registry(registry, username, password);
        var img = new ImageReference();
        img.HostName = registry;
        
        var hostPrefix  =  registry + "/";
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

        System.Console.WriteLine($"Getting manifest for {img.HostName}{img.Repository}:{img.Tag}");
        
        await reg.ShowManifestV2Async(img);
     
        return 0;
    }

    
}