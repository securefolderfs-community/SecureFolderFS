using CliFx;
using Microsoft.Extensions.DependencyInjection;
using SecureFolderFS.Sdk.Services;

namespace SecureFolderFS.Cli
{
    internal static class Program
    {
        private static async Task<int> Main(string[] args)
        {
            var normalizedArgs = args.Select(static x => x.Replace("--2fa-", "--twofa-", StringComparison.OrdinalIgnoreCase)).ToArray();

            var quiet = normalizedArgs.Any(x => string.Equals(x, "--quiet", StringComparison.OrdinalIgnoreCase)
                                   || string.Equals(x, "-q", StringComparison.OrdinalIgnoreCase));

            var lifecycle = new CliLifecycleHelper();
            var serviceProvider = lifecycle.BuildServiceProvider(quiet);
            try
            {
                await serviceProvider.GetRequiredService<ISettingsService>().InitAsync();
            }
            catch
            {
                // TODO: verify - consider a dedicated settings reset flow once CLI persistence format is finalized.
            }

            var app = new CliApplicationBuilder()
                .AddCommandsFromThisAssembly()
                .UseTypeActivator(type => ActivatorUtilities.CreateInstance(serviceProvider, type))
                .Build();

            return await app.RunAsync(normalizedArgs);
        }
    }
}
