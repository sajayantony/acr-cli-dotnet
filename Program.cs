using System.Threading.Tasks;
using System.CommandLine;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;


using System.CommandLine.Hosting;
using System.CommandLine.Builder;
using System.CommandLine.Parsing;
using Microsoft.Extensions.Logging;
using System.CommandLine.Binding;
using System.CommandLine.Invocation;
using System;
using AzureContainerRegistry.CLI.Services;

namespace AzureContainerRegistry.CLI
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var cmd = new AcrRootCommand();
            var builder = new CommandLineBuilder(new AcrRootCommand());            

            await builder.UseHost(_ => Host.CreateDefaultBuilder(),
                host =>
                {
                    InvocationContext context = (InvocationContext)host.Properties[typeof(InvocationContext)];

                    if (context.ParseResult.ValueForOption<bool>("verbose"))
                    {
                        host.ConfigureLogging(logging =>
                        {
                            logging.ClearProviders();
                            logging.AddConsole();
                        });
                    }
                    else
                    {
                        host.ConfigureLogging(logging =>
                        {
                            logging.ClearProviders();
                        });
                    }

                    host.ConfigureServices(services =>
                    {
                        var registry = context.ParseResult.ValueForOption<string>("registry");
                        var username = context.ParseResult.ValueForOption<string>("username");
                        var password = context.ParseResult.ValueForOption<string>("passworld");
                        password = !String.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("REGISTRY_PASSWORD")) ? 
                                        Environment.GetEnvironmentVariable("REGISTRY_PASSWORD") : password;

                        services.AddSingleton<Registry>(new Registry(registry, username, password));
                    });
                })
            .UseDefaults()
            .Build()
            .InvokeAsync(args);
        }


        public static IHostBuilder GetHost(InvocationContext invocationContext)
        {
            var modelBinder = new ModelBinder<IHostBuilder>();
            return (IHostBuilder)modelBinder.CreateInstance(invocationContext.BindingContext);
        }
    }
}
