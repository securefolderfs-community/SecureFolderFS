using CliFx;
using Microsoft.Extensions.DependencyInjection;
using SecureFolderFS.Shared;
using SecureFolderFS.Shared.Extensions;

namespace SecureFolderFS.Cli
{
    internal static class Program
    {
        public static CliLifecycleHelper Lifecycle { get; } = new();
        
        private static async Task<int> Main(string[] args)
        {
            await Lifecycle.InitAsync();
            DI.Default.SetServiceProvider(Lifecycle.ServiceCollection.BuildServiceProvider());
                
#if DEBUG
            if (args.IsEmpty())
            {
                // Initialize settings once, outside the loop
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

                    var app2 = new CliApplicationBuilder()
                        .AddCommandsFromThisAssembly()
                        .UseTypeActivator(type => ActivatorUtilities.CreateInstance(DI.Default, type))
                        .Build();

                    await app2.RunAsync(normalizedLoopArgs);
                    Console.WriteLine();
                }

                return 0;
            }
#endif
            var normalizedArgs = args.Select(static x => x.Replace("--2fa-", "--twofa-", StringComparison.OrdinalIgnoreCase)).ToArray();
            var app = new CliApplicationBuilder()
                .AddCommandsFromThisAssembly()
                .UseTypeActivator(type => ActivatorUtilities.CreateInstance(DI.Default, type))
                .Build();

            return await app.RunAsync(normalizedArgs);
        }
    }
}
