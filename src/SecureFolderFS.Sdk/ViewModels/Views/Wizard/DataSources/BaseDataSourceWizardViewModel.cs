using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using OwlCore.Storage;
using SecureFolderFS.Sdk.Enums;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.ViewModels.Views.Overlays;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Shared.Models;
using SecureFolderFS.Storage;

namespace SecureFolderFS.Sdk.ViewModels.Views.Wizard.DataSources
{
    [Bindable(true)]
    public abstract partial class BaseDataSourceWizardViewModel : OverlayViewModel, IStagingView
    {
        [ObservableProperty] private IImage? _Icon;

        protected IVaultCollectionModel VaultCollectionModel { get; }

        public NewVaultMode Mode { get; }

        public abstract string DataSourceName { get; }

        protected BaseDataSourceWizardViewModel(NewVaultMode mode, IVaultCollectionModel vaultCollectionModel)
        {
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

        public abstract Task<IFolder?> GetFolderAsync();
    }
}
