using System.CommandLine.Binding;
using System.CommandLine.Invocation;
using Microsoft.Extensions.Hosting;

namespace AzureContainerRegistry.CLI.Hosting
{

    static class Extensions
    {
        public static IHost GetHost(this InvocationContext invocationContext)
        {
            var modelBinder = new ModelBinder<IHost>();
            return (IHost)modelBinder.CreateInstance(invocationContext.BindingContext);
        }
    }
}