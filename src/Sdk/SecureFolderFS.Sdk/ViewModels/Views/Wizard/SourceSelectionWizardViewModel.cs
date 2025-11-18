using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SecureFolderFS.Sdk.Attributes;
using SecureFolderFS.Sdk.Enums;
using SecureFolderFS.Sdk.Extensions;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.ViewModels.Views.Overlays;
using SecureFolderFS.Sdk.ViewModels.Views.Wizard.DataSources;
using SecureFolderFS.Shared;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Shared.Extensions;
using SecureFolderFS.Shared.Models;

namespace SecureFolderFS.Sdk.ViewModels.Views.Wizard
{
    [Inject<IFileExplorerService>, Inject<IVaultFileSystemService>]
    [Bindable(true)]
    public sealed partial class SourceSelectionWizardViewModel : OverlayViewModel, IStagingView
    {
        private readonly NewVaultMode _mode;
        private readonly IStagingView _primaryStager;
        private readonly IVaultCollectionModel _vaultCollectionModel;

        [ObservableProperty] private BaseDataSourceWizardViewModel? _PrimarySource;
        [ObservableProperty] private BaseDataSourceWizardViewModel? _SelectedSource;
        [ObservableProperty] private ObservableCollection<BaseDataSourceWizardViewModel> _Sources;

        public SourceSelectionWizardViewModel(NewVaultMode mode, IStagingView primaryStager, IVaultCollectionModel vaultCollectionModel)
        {
            ServiceProvider = DI.Default;
            _mode = mode;
            _primaryStager = primaryStager;
            _vaultCollectionModel = vaultCollectionModel;
            Title = "ChooseDataSource".ToLocalized();
            Sources = new();
        }

        /// <inheritdoc/>
        public Task<IResult> TryContinueAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult<IResult>(Result.Success);
        }

        /// <inheritdoc/>
        public Task<IResult> TryCancelAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult<IResult>(Result.Success);
        }

        /// <inheritdoc/>
        public override async void OnAppearing()
        {
            if (!Sources.IsEmpty())
                return;

            var sources = await VaultFileSystemService.GetSourcesAsync(_vaultCollectionModel, _mode).ToArrayAsyncImpl();
            Sources.DisposeAll();
            Sources.Clear();
            Sources.AddMultiple(sources.Skip(1));

            // Set the primary source. In this case we use the first returned data source
            PrimarySource = sources.FirstOrDefault();
        }

        [RelayCommand]
        private async Task SelectSourceAsync(BaseDataSourceWizardViewModel? viewModel, CancellationToken cancellationToken)
        {
            if (viewModel is null)
                return;

            SelectedSource = viewModel;
            await _primaryStager.TryContinueAsync(cancellationToken);
        }
    }
}
