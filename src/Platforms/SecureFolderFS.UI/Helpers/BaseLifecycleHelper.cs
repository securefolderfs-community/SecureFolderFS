using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using OwlCore.Storage;
using OwlCore.Storage.System.IO;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.UI.ServiceImplementation;

namespace SecureFolderFS.UI.Helpers
{
    public abstract class BaseLifecycleHelper : IAsyncInitialize
    {
        public IServiceCollection ServiceCollection { get; } = new ServiceCollection();

        /// <inheritdoc/>
        public virtual Task InitAsync(CancellationToken cancellationToken = default)
        {
            var settingsFolderPath = Path.Combine(Directory.GetCurrentDirectory(), SecureFolderFS.UI.Constants.FileNames.SETTINGS_FOLDER_NAME);
            var settingsFolder = new SystemFolder(Directory.CreateDirectory(settingsFolderPath));
            ConfigureServices(settingsFolder);

            return Task.CompletedTask;
        }

        public virtual void LogException(Exception? ex)
        {
            var formattedException = ExceptionHelpers.FormatException(ex);
            Debug.WriteLine(formattedException);

            // Please check the "Output Window" for exception details (On Visual Studio, go to View -> Output Window or Ctr+Alt+O)
            Debugger.Break();
#if !DEBUG
            LogExceptionToFile(ex);
#endif
        }

        protected virtual IServiceCollection ConfigureServices(IModifiableFolder settingsFolder)
        {
            return ServiceCollection

                    // Singleton services
                    .AddSingleton<IVaultPersistenceService, VaultPersistenceService>(_ => new(settingsFolder))
                    .AddSingleton<IChangelogService, GitHubChangelogService>()

                ; // Finish service initialization
        }

        public abstract void LogExceptionToFile(Exception? ex);
    }
}
