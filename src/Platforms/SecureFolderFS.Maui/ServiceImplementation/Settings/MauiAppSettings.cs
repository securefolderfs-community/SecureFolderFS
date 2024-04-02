using OwlCore.Storage;
using SecureFolderFS.Sdk.Services.Settings;
using SecureFolderFS.UI.ServiceImplementation.Settings;

namespace SecureFolderFS.Maui.ServiceImplementation.Settings
{
    /// <inheritdoc cref="IAppSettings"/>
    internal sealed class MauiAppSettings : AppSettings
    {
        public MauiAppSettings(IModifiableFolder settingsFolder)
            : base(settingsFolder)
        {
        }

        /// <inheritdoc/>
        public override bool ShouldShowVaultTutorial
        {
            get => false; // Don't show vault tutorial on Maui
            set { }
        }
    }
}
