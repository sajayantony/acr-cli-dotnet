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
        repoListCommand.Handler = CommandHandler.Create<IHost>(host =>
       {
           var registry = host.Services.GetRequiredService<RegistryService>();
           Console.WriteLine("Repositories");
           Console.WriteLine("------------");
           return ListRepositoryAsync(registry);
       });


        var tagListCommand = new Command("list-tags"){
                 new Option<string>(
                    "--repository",
                    description: "Repository name")
            };

        tagListCommand.Handler = CommandHandler.Create<string, IHost>((repository, host) =>
        {
            Console.WriteLine("Tags");
            Console.WriteLine("----");
            var registry = host.Services.GetRequiredService<RegistryService>();
            return ListTagsAsync(registry, repository);
        });

        // Add repository commands            
        this.Add(repoListCommand);
        this.Add(tagListCommand);
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

    static async Task<int> ListTagsAsync(RegistryService registry, string repo)
    {
        var tagList = await registry.ListTagsAsync(repo);
        foreach (var tag in tagList.Tags)
        {
            Console.WriteLine(tag.Name);
        }

        return 0;
    }
}