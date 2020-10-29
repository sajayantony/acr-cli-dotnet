using System;
using System.Threading.Tasks;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.Builder;


namespace AzureContainerRegistry.CLI
{
    class Program
    {
        static readonly string DEFAULT_PASSWORD = new String('*', 5);

        class CommandRegistryContext
        {

            public string Registry { get; set; }
            public string Username { get; set; }
            public string Password { get; set; }

        }

        static async Task Main(string[] args)
        {

            var ctx = new CommandRegistryContext();
            var repoCommand = new Command("repository");

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

            repoCommand.Add(repoListCommand);
            repoCommand.Add(tagListCommand);

            var rootCommand = new RootCommand
            {
                repoCommand
            };

            rootCommand.Description = System.Environment.GetCommandLineArgs()[0];


            rootCommand.AddGlobalOption(
                new Option<string>(
                    "--registry",
                    getDefaultValue: () => Environment.GetEnvironmentVariable("REGISTRY_LOGIN"),
                    "Registry Login Server")
            );

            rootCommand.AddGlobalOption(
                 new Option<string>(
                       "--username",
                       getDefaultValue: () => Environment.GetEnvironmentVariable("REGISTRY_USERNAME"),
                       "Registry Username")
            );

            rootCommand.AddGlobalOption(
                 new Option<string>(
                       "--password",
                       getDefaultValue: () => Environment.GetEnvironmentVariable("REGISTRY_PASSWORD").Length > 0 ? DEFAULT_PASSWORD : null,
                       "Registry Login Server")
            );

            new CommandLineBuilder(rootCommand).UseMiddleware(async (context, next) =>
            {
                ctx.Registry = context.ParseResult.ValueForOption<string>("registry");
                ctx.Username = context.ParseResult.ValueForOption<string>("username");

                var password = context.ParseResult.ValueForOption<string>("passworld");
                ctx.Password = Environment.GetEnvironmentVariable("REGISTRY_PASSWORD").Length > 0 ? Environment.GetEnvironmentVariable("REGISTRY_PASSWORD") : password;
                await next(context);
            }).UseDefaults()
            .UseHelp()
            .Build();

            await rootCommand.InvokeAsync(args);
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
}
