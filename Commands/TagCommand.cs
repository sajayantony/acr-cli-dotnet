using System;
using System.CommandLine;
using System.CommandLine.Parsing;
using System.CommandLine.Invocation;
using System.Threading.Tasks;
using AzureContainerRegistry.CLI.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OCI;

namespace AzureContainerRegistry.CLI.Commands
{
    class TagCommand : Command
    {
        private ILogger _logger;

        public TagCommand() : base("tag", "Tag operations")
        {
            var addCmd = new Command("add");
            addCmd.AddArgument(new Argument<string>("source", "Source image reference myregistry.azurecr.io/repos:source"));


            var tagArg = new Argument<string>("tag", "Target tag which will be placed in the same repository.");

            tagArg.AddValidator(r =>
                {
                    
                    var value = r.GetValueOrDefault<string>();
                    Console.WriteLine("Validating arg " + value);
                    if (!OCI.RegularExpressions.Regexp.IsValidTag(value))
                    {
                        return $"Tag specified is invalid: {value}";
                    }
                    return null;
                });

            addCmd.AddArgument(tagArg);


            addCmd.Handler = CommandHandler.Create<string, string, IHost>(async (source, tag, host) =>
             {
                 var loggerFactory = host.Services.GetRequiredService<ILoggerFactory>();
                 _logger = loggerFactory.CreateLogger(typeof(ManifestCommand));
                 var registry = host.Services.GetRequiredService<RegistryService>();
                 await AddTagAsync(registry, source, tag);
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

        async Task AddTagAsync(RegistryService reg, string srcRef, string tag)
        {
            var src = srcRef.ToArtifactReference();
            var dest = $"{src.HostName}/{src.Repository}:{tag}".ToArtifactReference();
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