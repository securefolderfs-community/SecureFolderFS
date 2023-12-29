﻿using Microsoft.Extensions.DependencyInjection;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.Storage;
using SecureFolderFS.Sdk.Storage.ModifiableStorage;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.UI.ServiceImplementation;
using SecureFolderFS.UI.Storage.NativeStorage;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.UI.Helpers
{
    public abstract class BaseLifecycleHelper : IAsyncInitialize
    {
        public IServiceCollection ServiceCollection { get; } = new ServiceCollection();

        /// <inheritdoc/>
        public virtual Task InitAsync(CancellationToken cancellationToken = default)
        {
            var settingsFolderPath = Path.Combine(Directory.GetCurrentDirectory(), SecureFolderFS.UI.Constants.FileNames.SETTINGS_FOLDER_NAME);
            var settingsFolder = new NativeFolder(Directory.CreateDirectory(settingsFolderPath));
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

        public abstract void LogExceptionToFile(Exception? ex);

        protected virtual IServiceCollection ConfigureServices(IModifiableFolder settingsFolder)
        {
            return ServiceCollection

                // Singleton services
                .AddSingleton<ISettingsService, SettingsService>(_ => new(settingsFolder))
                .AddSingleton<IVaultPersistenceService, VaultPersistenceService>(_ => new(settingsFolder))
                .AddSingleton<IVaultService, VaultService>()
                .AddSingleton<IChangelogService, GitHubChangelogService>()
                .AddSingleton<IStorageService, NativeStorageService>()

                ; // Finish service initialization
        }
    }
}
