using System;
using System.ComponentModel;
using System.IO;
using System.IO.Compression;
using System.Threading;
using System.Threading.Tasks;
using OwlCore.Storage;
using SecureFolderFS.Sdk.AppModels;
using SecureFolderFS.Sdk.AppModels.Database;
using SecureFolderFS.Sdk.Enums;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.Services.Settings;
using SecureFolderFS.Shared;
using SecureFolderFS.Storage.Extensions;

namespace SecureFolderFS.UI.ServiceImplementation.Settings
{
    /// <inheritdoc cref="IUserSettings"/>
    public class UserSettings : SettingsModel, IUserSettings
    {
        private readonly IModifiableFolder _settingsFolder;
        private ITelemetryService TelemetryService { get; } = DI.Service<ITelemetryService>();

        /// <inheritdoc/>
        protected override IDatabaseModel<string> SettingsDatabase { get; }

        public UserSettings(IModifiableFolder settingsFolder)
        {
            _settingsFolder = settingsFolder;
            SettingsDatabase = new SingleFileDatabaseModel(Constants.FileNames.USER_SETTINGS_FILENAME, settingsFolder, DoubleSerializedStreamSerializer.Instance);
            PropertyChanged += UserSettings_PropertyChanged;
        }

        #region Preferences

        /// <inheritdoc/>
        public virtual string PreferredFileSystemId
        {
            get => GetSetting(static () => Core.Constants.FileSystemId.FS_WEBDAV);
            set => SetSetting(value);
        }

        /// <inheritdoc/>
        public virtual bool StartOnSystemStartup
        {
            get => GetSetting(static () => false);
            set => SetSetting(value);
        }

        /// <inheritdoc/>
        public virtual bool ReduceToBackground
        {
            get => GetSetting(static () => true);
            set => SetSetting(value);
        }

        /// <inheritdoc/>
        public virtual bool ContinueOnLastVault
        {
            get => GetSetting(static () => false);
            set => SetSetting(value);
        }

        /// <inheritdoc/>
        public virtual bool OpenFolderOnUnlock
        {
            get => GetSetting(static () => true);
            set => SetSetting(value);
        }

        #endregion

        #region File Browser

        /// <inheritdoc/>
        public bool AreThumbnailsEnabled
        {
            get => GetSetting(static () => true);
            set => SetSetting(value);
        }

        /// <inheritdoc/>
        public bool AreFileExtensionsEnabled
        {
            get => GetSetting(static () => true);
            set => SetSetting(value);
        }

        /// <inheritdoc/>
        public bool IsAdaptiveLayoutEnabled
        {
            get => GetSetting(static () => true);
            set => SetSetting(value);
        }

        /// <inheritdoc/>
        public bool IsContentCacheEnabled
        {
            get => GetSetting(static () => true);
            set => SetSetting(value);
        }

        #endregion

        #region Privacy

        /// <inheritdoc/>
        public virtual bool IsTelemetryEnabled
        {
            get => GetSetting(static () => true);
            set => SetSetting(value);
        }

        /// <inheritdoc/>
        public virtual bool LockOnSystemLock
        {
            get => GetSetting(static () => false);
            set => SetSetting(value);
        }

        /// <inheritdoc/>
        public virtual bool DisableRecentAccess
        {
            get => GetSetting(static () => false);
            set => SetSetting(value);
        }

        #endregion

        protected virtual void UserSettings_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            // Opt out
            return;

            var eventName = e.PropertyName switch
            {
                nameof(LockOnSystemLock) => $"{nameof(LockOnSystemLock)}: {LockOnSystemLock}",
                nameof(IsTelemetryEnabled) => $"{nameof(IsTelemetryEnabled)}: {IsTelemetryEnabled}",
                nameof(PreferredFileSystemId) => $"{nameof(PreferredFileSystemId)}: {PreferredFileSystemId}",
                nameof(StartOnSystemStartup) => $"{nameof(StartOnSystemStartup)}: {StartOnSystemStartup}",
                nameof(ContinueOnLastVault) => $"{nameof(ContinueOnLastVault)}: {ContinueOnLastVault}",
                nameof(OpenFolderOnUnlock) => $"{nameof(OpenFolderOnUnlock)}: {OpenFolderOnUnlock}",
                nameof(ReduceToBackground) => $"{nameof(ReduceToBackground)}: {ReduceToBackground}",
                nameof(AreThumbnailsEnabled) => $"{nameof(AreThumbnailsEnabled)}: {AreThumbnailsEnabled}",
                nameof(IsContentCacheEnabled) => $"{nameof(IsContentCacheEnabled)}: {IsContentCacheEnabled}",
                _ => null
            };

            if (eventName is not null)
                TelemetryService.TrackMessage(eventName, Severity.Default);
        }

        /// <inheritdoc/>
        public virtual async Task<Stream> ExportAsync(CancellationToken cancellationToken = default)
        {
            // Ensure settings are saved before exporting
            await SaveAsync(cancellationToken);

            // Get the settings file
            var settingsFile = await _settingsFolder.TryGetFileByNameAsync(Constants.FileNames.USER_SETTINGS_FILENAME, cancellationToken) as IFile;
            if (settingsFile is null)
                return Stream.Null;

            // Create a memory stream to hold the zip archive
            var memoryStream = new MemoryStream();
            await using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, leaveOpen: true))
            {
                // Create an entry with the specified filename
                var entry = archive.CreateEntry(Constants.FileNames.USER_SETTINGS_FILENAME, CompressionLevel.Optimal);
                
                await using var entryStream = await entry.OpenAsync(cancellationToken);
                await using var settingsStream = await settingsFile.OpenReadAsync(cancellationToken);
                await settingsStream.CopyToAsync(entryStream, cancellationToken);
            }

            // Reset the position for reading
            memoryStream.Position = 0;
            return memoryStream;
        }

        /// <inheritdoc/>
        public virtual async Task<bool> ImportAsync(Stream dataStream, CancellationToken cancellationToken = default)
        {
            try
            {
                await using var archive = new ZipArchive(dataStream, ZipArchiveMode.Read, leaveOpen: true);
                
                // Find the settings file in the archive
                var entry = archive.GetEntry(Constants.FileNames.USER_SETTINGS_FILENAME);
                if (entry is null)
                    return false;

                // Get or create the settings file
                var settingsFile = await _settingsFolder.CreateFileAsync(Constants.FileNames.USER_SETTINGS_FILENAME, true, cancellationToken);

                // Write the imported settings
                await using var entryStream = await entry.OpenAsync(cancellationToken);
                await using (var settingsStream = await settingsFile.OpenWriteAsync(cancellationToken))
                {
                    // Clear existing content
                    settingsStream.SetLength(0);
                    await entryStream.CopyToAsync(settingsStream, cancellationToken);
                    await settingsStream.FlushAsync(cancellationToken);
                }

                // Reinitialize to load the new settings
                await InitAsync(cancellationToken);

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
