using System.CommandLine.Binding;
using System.CommandLine.Invocation;
using Microsoft.Extensions.Hosting;
using OCI;

namespace AzureContainerRegistry.CLI
{

    static class Extensions
    {
        public static ImageReference ToImageReference(this string reference, string registry)
        {
            var img = new ImageReference();
            img.HostName = registry;

            var hostPrefix = img.HostName + "/";
            if (reference.StartsWith(hostPrefix))
            {
                //Trim the registry to get repository and tag. 
                reference = reference.Substring(hostPrefix.Length);
                if (reference.Contains("@sha256"))
                {
                    var parts = reference.Split('@');
                    if (parts.Length > 1)
                    {
                        img.Repository = parts[0];
                        img.Digest = parts[1];
                    }

                }
                else if (reference.Contains(':'))
                {
                    var parts = reference.Split(':');
                    if (parts.Length > 1)
                    {
                        img.Repository = parts[0];
                        img.Tag = parts[1];
                    }
                }
            }

            if (string.IsNullOrEmpty(img.Digest) && string.IsNullOrEmpty(img.Tag))
            {
                throw new System.Exception($"Invalid Image format: {reference}");
            }

            return img;
        }
    }
}