using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using OwlCore.Storage;
using OwlCore.Storage.System.IO;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.UI.Helpers;
using SecureFolderFS.UI.ServiceImplementation;
using SecureFolderFS.Uno.Extensions;
using SecureFolderFS.Uno.Platforms.Windows.ServiceImplementation;
using Windows.Storage;
using SecureFolderFS.Shared.Extensions;
using AddService = Microsoft.Extensions.DependencyInjection.ServiceCollectionServiceExtensions;

namespace SecureFolderFS.Uno.Platforms.Windows.Helpers
{
    /// <inheritdoc cref="BaseLifecycleHelper"/>
    internal sealed class WindowsLifecycleHelper : BaseLifecycleHelper
    {
        /// <inheritdoc/>
        public override string AppDirectory { get; } = ApplicationData.Current.LocalFolder.Path;

        /// <inheritdoc/>
        public override Task InitAsync(CancellationToken cancellationToken = default)
        {
            // Initialize settings
            var settingsFolderPath = Path.Combine(AppDirectory, SecureFolderFS.UI.Constants.FileNames.SETTINGS_FOLDER_NAME);
            var settingsFolder = new SystemFolder(Directory.CreateDirectory(settingsFolderPath));
            ConfigureServices(settingsFolder);

            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public override void LogExceptionToFile(Exception? ex)
        {
            var formattedException = ExceptionHelpers.FormatException(ex);
            if (formattedException is null)
                return;

            ExceptionHelpers.WriteSessionFile(AppDirectory, formattedException);
            ExceptionHelpers.WriteAggregateFile(AppDirectory, formattedException);
        }

        /// <inheritdoc/>
        protected override IServiceCollection ConfigureServices(IModifiableFolder settingsFolder)
        {
            return base.ConfigureServices(settingsFolder)
                .Override<ISystemService, WindowsSystemService>(AddService.AddSingleton)
                .Override<IPrinterService, WindowsPrinterService>(AddService.AddSingleton)
                .Override<IApplicationService, WindowsApplicationService>(AddService.AddSingleton)
                .Override<IVaultFileSystemService, WindowsVaultFileSystemService>(AddService.AddSingleton)
                .Override<IVaultCredentialsService, WindowsVaultCredentialsService>(AddService.AddSingleton)

                // IIapService, IUpdateService
#if DEBUG || UNPACKAGED
                .Override<IIapService, DebugIapService>(AddService.AddSingleton)
                .Override<IUpdateService, DebugUpdateService>(AddService.AddSingleton)
#else
                .Override<IIapService, DebugIapService>(AddService.AddSingleton) // .AddSingleton<IIapService, MicrosoftStoreIapService>() // TODO: Change in the future
                .Override<IUpdateService, MicrosoftStoreUpdateService>(AddService.AddSingleton)
#endif

                // ILocalizationService
#if UNPACKAGED
                .Override<ILocalizationService, ResourceLocalizationService>(AddService.AddSingleton)
#else
                .Override<ILocalizationService, WindowsLocalizationService>(AddService.AddSingleton)
#endif
                
                .WithUnoServices(settingsFolder)
                ;
        }
    }
}
