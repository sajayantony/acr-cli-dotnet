using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Threading.Tasks;
using AzureContainerRegistry.CLI;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using AzureContainerRegistry.CLI.Services;

class RepositoryCommand : Command
{
    public RepositoryCommand() : base("repository", "Repository operations")
    {
        var repoListCommand = new Command("list");
        repoListCommand.Handler = CommandHandler.Create<IHost>(async (host) =>
       {
           var registry = host.Services.GetRequiredService<RegistryService>();
           Console.WriteLine("Repositories");
           Console.WriteLine("------------");
           await ListRepositoryAsync(registry);
       });


        // Add repository commands            
        this.Add(repoListCommand);
    }

    static async Task<int> ListRepositoryAsync(RegistryService reg)
    {
        var repos = await reg.ListRespositoriesAsync();
        foreach (var repo in repos.Names)
        {
            Console.WriteLine(repo);
        }

        return 0;
    }

}