using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Threading.Tasks;
using AzureContainerRegistry.CLI;

class RepositoryCommand : Command
{
    public RepositoryCommand(CommandRegistryContext ctx) : base("repository", "Repository operations")
    {
        var repoListCommand = new Command("list");
        repoListCommand.Handler = CommandHandler.Create(() =>
        {
            Console.WriteLine("Repositories");
            Console.WriteLine("------------");
            return ListRepositoryAsync(ctx.Registry, ctx.Username, ctx.Password);
        });


        var tagListCommand = new Command("list-tags"){
                 new Option<string>(
                    "--repository",
                    description: "Repository name")
            };

        tagListCommand.Handler = CommandHandler.Create<string>((repository) =>
        {
            Console.WriteLine("Tags");
            Console.WriteLine("----");
            return ListTagsAsync(ctx.Registry, ctx.Username, ctx.Password, repository);
        });

        // Add repository commands            
        this.Add(repoListCommand);
        this.Add(tagListCommand);
    }

    static async Task<int> ListRepositoryAsync(string registry, string username, string password)
    {
        var reg = new Registry(registry, username, password);
        var repos = await reg.ListRespositoriesAsync();
        foreach (var repo in repos.Names)
        {
            Console.WriteLine(repo);
        }

        return 0;
    }

    static async Task<int> ListTagsAsync(string registry, string username, string password, string repo)
    {

        var reg = new Registry(registry, username, password);
        var tagList = await reg.ListTagsAsync(repo);
        foreach (var tag in tagList.Tags)
        {
            Console.WriteLine(tag.Name);
        }

        return 0;
    }
}