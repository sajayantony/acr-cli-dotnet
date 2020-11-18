using System;
using System.Threading.Tasks;
using System.CommandLine;
using System.CommandLine.Hosting;
using System.CommandLine.Builder;
using System.CommandLine.Parsing;
using System.CommandLine.Binding;
using System.CommandLine.Invocation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using AzureContainerRegistry.CLI.Services;
using AzureContainerRegistry.CLI.Commands;

namespace AzureContainerRegistry.CLI
{
    class Program
    {
        static async Task Main(string[] args)
        {
            try
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
                            var reference = context.ParseResult.ValueForOption<string>("registry");
                            password = !String.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("REGISTRY_PASSWORD")) ?
                                            Environment.GetEnvironmentVariable("REGISTRY_PASSWORD") : password;

                            if (!String.IsNullOrEmpty(username) &&
                                !string.IsNullOrEmpty(password) &&
                                !string.IsNullOrEmpty(registry))
                            {
                                services.AddSingleton<CredentialsProvider>(
                                        _ => new CredentialsProvider(registry, username, password));
                            }
                            else
                            {
                                var creds = new CredentialsProvider();
                                creds.TryInitialize(reference);
                                services.AddSingleton<CredentialsProvider>( _ = creds);
                            }

                            services.AddSingleton(typeof(RegistryService));
                            services.AddSingleton(typeof(ContentStore));
                            services.AddSingleton<System.IO.TextWriter>(System.Console.Out);
                        });
                    })
                .UseVersionOption()
                .UseHelp()
                .UseEnvironmentVariableDirective()
                .UseDebugDirective()
                .UseSuggestDirective()
                .RegisterWithDotnetSuggest()
                .UseTypoCorrections()
                .UseParseErrorReporting()
                .CancelOnProcessTermination()
                //.UseExceptionHandler()
                .Build()
                .InvokeAsync(args);
            }
            catch (Exception ex)
            {
                System.Console.WriteLine("Error: " + ex.Message);
            }
        }
    }
}
