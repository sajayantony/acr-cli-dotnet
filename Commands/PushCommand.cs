using System.CommandLine;
using System.CommandLine.Invocation;
using AzureContainerRegistry.CLI.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;


namespace AzureContainerRegistry.CLI.Commands
{
    class PushCommand : Command
    {
        public PushCommand() : base("push", "Push an artifact")
        {
            this.AddArgument(new Argument<string>("reference"));

            this.Add(new Option<string>(new string[] { "--directory", "-d" },
                    description: "Directory path of the contents to upload which containts manifest.json and config.json and other tar files."));

            this.Add(new Option<string>(new string[] { "--file", "-f" },
                       description: "File path of the content to upload"));

            this.Add(new Option<string>("--config-media-type",
                       description: "File path of the content to upload"));

            this.AddValidator(r =>
            {
                // File and directly are mutually exclusive and one needs to be specified.

                var directory = r.ValueForOption<string>("directory");
                var file = r.ValueForOption<string>("file");
                var configMediaType = r.ValueForOption<string>("config-media-type");

                if (string.IsNullOrEmpty(file) && string.IsNullOrEmpty(directory))
                {
                    return "Either --file or --directory is required";
                }

                if (!string.IsNullOrEmpty(file) && !string.IsNullOrEmpty(directory))
                {
                    return "Both --file and --directory cannot be specified";
                }

                if (!string.IsNullOrEmpty(directory) && !string.IsNullOrEmpty(configMediaType))
                {
                    return "--config-media-type can be specified only with --file";
                }

                return null;
            });

            this.Handler = CommandHandler.Create<string, string, string, string, IHost>(async (reference, directory, file, configMediaType, host) =>
             {
                 var contentStore = host.Services.GetRequiredService<ContentStore>();
                 if (!string.IsNullOrEmpty(directory))
                 {
                     await contentStore.PushAsync(reference.ToArtifactReference(), directory);
                 }
                 else
                 {
                     await contentStore.PushAsync(reference.ToArtifactReference(), configMediaType, file);
                 }
             });
        }
    }
}
