using CliFx;
using Microsoft.Extensions.DependencyInjection;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Shared.Extensions;

namespace SecureFolderFS.Cli
{
    internal static class Program
    {
        private static async Task<int> Main(string[] args)
        {
#if DEBUG
            if (args.IsEmpty())
            {
                // Initialize settings once, outside the loop
                var initLifecycle = new CliLifecycleHelper();
                var initProvider = initLifecycle.BuildServiceProvider(quiet: false);
                try
                {
                    await initProvider.GetRequiredService<ISettingsService>().InitAsync();
                }
                catch { }

                Console.WriteLine("Interactive CLI mode. Press Enter with no input to exit.");
                while (true)
                {
                    Console.Write("> ");
                    var input = Console.ReadLine();
                    if (string.IsNullOrWhiteSpace(input))
                        break;

                    var loopArgs = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    var normalizedLoopArgs = loopArgs
                        .Select(static x => x.Replace("--2fa-", "--twofa-", StringComparison.OrdinalIgnoreCase))
                        .ToArray();

                    // Rebuild service provider per iteration to avoid double-registration
                    var lifecycle2 = new CliLifecycleHelper();
                    var serviceProvider2 = lifecycle2.BuildServiceProvider(
                        normalizedLoopArgs.Any(x =>
                            string.Equals(x, "--quiet", StringComparison.OrdinalIgnoreCase) ||
                            string.Equals(x, "-q", StringComparison.OrdinalIgnoreCase)));

                    var app2 = new CliApplicationBuilder()
                        .AddCommandsFromThisAssembly()
                        .UseTypeActivator(type => ActivatorUtilities.CreateInstance(serviceProvider2, type))
                        .Build();

                    await app2.RunAsync(normalizedLoopArgs);
                    Console.WriteLine();
                }

                return 0;
            }
#endif
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
