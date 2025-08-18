using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using OwlCore.Storage;
using SecureFolderFS.Sdk.DataModels;
using SecureFolderFS.Sdk.Enums;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.ViewModels.Views.Overlays;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Shared.Models;
using SecureFolderFS.Storage;

namespace SecureFolderFS.Sdk.ViewModels.Views.Wizard.DataSources
{
    [Bindable(true)]
    public abstract partial class BaseDataSourceWizardViewModel : OverlayViewModel, IStagingView, IDisposable
    {
        [ObservableProperty] private IImage? _Icon;

        protected IVaultCollectionModel VaultCollectionModel { get; }

        public NewVaultMode Mode { get; }

        public string DataSourceType { get; }

        public abstract string DataSourceName { get; }

        protected BaseDataSourceWizardViewModel(string dataSourceType, NewVaultMode mode, IVaultCollectionModel vaultCollectionModel)
        {
            DataSourceType = dataSourceType;
            Mode = mode;
            VaultCollectionModel = vaultCollectionModel;
        }

        /// <inheritdoc/>
        public virtual async Task<IResult> TryContinueAsync(CancellationToken cancellationToken)
        {
            var selectedFolder = await GetFolderAsync();
            if (selectedFolder is null)
                return Result.Failure(null);

            // Confirm bookmark
            if (selectedFolder is IBookmark bookmark)
                await bookmark.AddBookmarkAsync(cancellationToken);

            return Result.Success;
        }

        /// <inheritdoc/>
        public override void OnDisappearing()
        {
            Icon?.Dispose();
        }

        /// <inheritdoc/>
        public abstract Task<IResult> TryCancelAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Gets the selected folder by the user.
        /// </summary>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. The task result contains the selected folder or null if no folder is selected.</returns>
        public abstract Task<IFolder?> GetFolderAsync();

        /// <summary>
        /// Gets the appropriate <see cref="VaultStorageSourceDataModel"/> representation.
        /// </summary>
        /// <returns>A <see cref="VaultStorageSourceDataModel"/> representing the storage source.</returns>
        public abstract VaultStorageSourceDataModel? ToStorageSource();

        /// <inheritdoc/>
        public abstract void Dispose();
    }
}
