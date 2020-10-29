using System.CommandLine.Binding;
using System.CommandLine.Invocation;
using Microsoft.Extensions.Hosting;
using OCI;

namespace AzureContainerRegistry.CLI
{

    static class Extensions
    {
        public static IHost GetHost(this InvocationContext invocationContext)
        {
            var modelBinder = new ModelBinder<IHost>();
            return (IHost)modelBinder.CreateInstance(invocationContext.BindingContext);
        }


        public static ImageReference ToImageReference(this string reference, string registry)
        {
            var img = new ImageReference();
            img.HostName = registry;

            var hostPrefix = img.HostName + "/";
            if (reference.StartsWith(hostPrefix))
            {
                //Trim the registry to get repository and tag. 
                reference = reference.Substring(hostPrefix.Length);
                if (reference.Contains(':'))
                {
                    var parts = reference.Split(':');
                    img.Repository = parts[0];
                    img.Tag = parts[1];
                }
            }

            return img;
        }
    }
}