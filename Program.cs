using System;
using System.Threading.Tasks;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.Builder;


namespace AzureContainerRegistry.CLI
{

    
        class CommandRegistryContext
        {
            public string Registry { get; set; }
            public string Username { get; set; }
            public string Password { get; set; }
        }
    class Program
    {
        static readonly string DEFAULT_PASSWORD = new String('*', 5);
        static CommandRegistryContext ctx = new CommandRegistryContext();


        static async Task Main(string[] args)
        {

           // Start building root command
            var rootCommand = new RootCommand
            {
                new RepositoryCommand(ctx), 
                new ManifestCommand(ctx)
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
            })
            .UseDefaults()
            .UseHelp()
            .Build();

            await rootCommand.InvokeAsync(args);
        }
    }
}
