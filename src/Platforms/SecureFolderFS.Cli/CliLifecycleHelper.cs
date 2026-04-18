using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OwlCore.Storage;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Shared.Extensions;
using SecureFolderFS.UI.Helpers;
using SecureFolderFS.UI.ServiceImplementation;
using AddService = Microsoft.Extensions.DependencyInjection.ServiceCollectionServiceExtensions;

namespace SecureFolderFS.Cli
{
    internal sealed class CliLifecycleHelper : BaseLifecycleHelper
    {
        /// <inheritdoc/>
        public override string AppDirectory { get; } = Directory.GetCurrentDirectory();

        /// <inheritdoc/>
        public override void LogExceptionToFile(Exception? ex)
        {
            _ = ex; // No-op
        }

        /// <inheritdoc/>
        protected override IServiceCollection ConfigureServices(IModifiableFolder settingsFolder)
        {
            return base.ConfigureServices(settingsFolder)
                .Override<IVaultFileSystemService, CliVaultFileSystemService>(AddService.AddSingleton)
                .Override<IVaultCredentialsService, CliVaultCredentialsService>(AddService.AddSingleton)
                .Override<ITelemetryService, DebugTelemetryService>(AddService.AddSingleton)
                .Override<CredentialReader, CredentialReader>(AddService.AddSingleton);
        }

        /// <inheritdoc/>
        protected override IServiceCollection WithLogging(IServiceCollection serviceCollection)
        {
            return serviceCollection
                .AddLogging(builder =>
                {
                    builder.ClearProviders();
                    builder.SetMinimumLevel(LogLevel.Information);
                    builder.AddSimpleConsole(options =>
                    {
                        options.SingleLine = true;
                        options.TimestampFormat = "HH:mm:ss ";
                    });
                });
        }
    }
}
