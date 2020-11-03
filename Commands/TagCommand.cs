using System;
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
            addCmd.AddArgument(new Argument<string>("source", "Source image reference myregistry.azurecr.io/repos:source"));
            addCmd.AddArgument(new Argument<string>("target", "Target tag which will be placed in the same repository."));
            addCmd.Handler = CommandHandler.Create<string, string, IHost>(async (source, target, host) =>
             {
                 var loggerFactory = host.Services.GetRequiredService<ILoggerFactory>();
                 _logger = loggerFactory.CreateLogger(typeof(ManifestCommand));
                 var registry = host.Services.GetRequiredService<RegistryService>();
                 await AddTagAsync(registry, source, target);
             });

            var deleteCmd = new Command("delete", "Removes the tag and does not delete the image.");
            deleteCmd.AddArgument(new Argument<string>("reference", "Fully qualified myregistry.azurecr.io/repo:tag or myregistry.azurecr.io/repo@sha2568fd4d2d7"));
            deleteCmd.Handler = CommandHandler.Create<string, IHost>((reference, host) =>
            {
                System.Console.WriteLine("To Be Implemented");
            });

            var tagListCmd = new Command("list");
            tagListCmd.AddArgument(new Argument<string>("repository", "Repository to list tags of."));

            tagListCmd.Handler = CommandHandler.Create<string, IHost>(async (repository, host) =>
            {
                Console.WriteLine("Tags");
                Console.WriteLine("----");
                var registry = host.Services.GetRequiredService<RegistryService>();
                await ListTagsAsync(registry, repository);
            });

            this.Add(tagListCmd);
            this.Add(addCmd);
            this.Add(deleteCmd);
        }

        async Task AddTagAsync(RegistryService reg, string srcRef, string destRef)
        {
            var src = srcRef.ToImageReference(reg.LoginServer);
            var dest = destRef.ToImageReference(reg.LoginServer);
            await reg.AddTagAsync(src, dest);            
        }

        static async Task ListTagsAsync(RegistryService registry, string repo)
        {
            var tagList = await registry.ListTagsAsync(repo);
            foreach (var tag in tagList.Tags)
            {
                Console.WriteLine(tag.Name);
            }            
        }
    }
}