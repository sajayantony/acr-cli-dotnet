using System.CommandLine.Binding;
using System.CommandLine.Invocation;
using Microsoft.Extensions.Hosting;
using OCI;

namespace AzureContainerRegistry.CLI
{
    static class Extensions
    {
        public static ArtifactReference ToArtifactReference(this string reference)
        {
            System.Console.WriteLine(reference);
            var img = new ArtifactReference();

            //Check see if registry hostname is provided. 
            var index = reference.IndexOf('/');
            if (index > -1)
            {
                // myregistry.azurecr.io/hello-world:latest
                img.HostName = reference.Substring(0, index);
            }

            var hostPrefix = img.HostName + "/";
            //Trim the registry to get repository and tag. 
            var refPart = reference.Substring(hostPrefix.Length);
            if (refPart.Contains("@sha256"))
            {
                var parts = refPart.Split('@');
                if (parts.Length > 1)
                {
                    img.Repository = parts[0];
                    img.Digest = parts[1];
                }
            }
            else if (refPart.Contains(':'))
            {
                var parts = refPart.Split(':');
                if (parts.Length > 1)
                {
                    img.Repository = parts[0];
                    img.Tag = parts[1];
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