using System;
using System.CommandLine;
using System.CommandLine.Builder;

namespace AzureContainerRegistry.CLI
{

    class CommandRegistryContext
    {
        public string Registry { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
    }

    class AcrRootCommand : RootCommand
    {
        static readonly string DEFAULT_PASSWORD = new String('*', 5);

        public AcrRootCommand()
        {

            this.Description = System.Environment.GetCommandLineArgs()[0];

            this.AddGlobalOption(
                new Option<string>(
                    "--registry",
                    getDefaultValue: () => Environment.GetEnvironmentVariable("REGISTRY_LOGIN"),
                    "Registry Login Server")
            );

            this.AddGlobalOption(
                 new Option<string>(
                       "--username",
                       getDefaultValue: () => Environment.GetEnvironmentVariable("REGISTRY_USERNAME"),
                       "Registry Username")
            );

            this.AddGlobalOption(
                 new Option<string>(
                       "--password",
                       getDefaultValue: () => !String.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("REGISTRY_PASSWORD")) ? DEFAULT_PASSWORD : null,
                       "Registry Login Server")
            );

            this.AddGlobalOption(
                 new Option<bool>(
                       "--verbose",
                       getDefaultValue: () => false,
                       "Enable verbose logging")
            );

            this.AddCommand(new RepositoryCommand());
            this.AddCommand(new ManifestCommand());
            this.AddCommand(new TagCommand());
            this.AddCommand(new PullCommand());

        }
    }
}