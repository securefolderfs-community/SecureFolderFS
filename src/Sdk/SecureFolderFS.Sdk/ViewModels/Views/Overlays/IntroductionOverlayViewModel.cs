using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SecureFolderFS.Sdk.Attributes;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.ViewModels.Controls.Components;
using SecureFolderFS.Shared;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Shared.Extensions;
using SecureFolderFS.Storage.Enums;

namespace SecureFolderFS.Sdk.ViewModels.Views.Overlays
{
    [Inject<IOverlayService>, Inject<ISettingsService>, Inject<IVaultFileSystemService>]
    [Bindable(true)]
    public sealed partial class IntroductionOverlayViewModel : OverlayViewModel, IAsyncInitialize
    {
        [ObservableProperty] private int _CurrentIndex;
        [ObservableProperty] private string? _CurrentStep;
        [ObservableProperty] private PickerOptionViewModel? _SelectedFileSystem;
        [ObservableProperty] private ObservableCollection<PickerOptionViewModel> _FileSystems;

        public int SlidesCount { get; set; }

        public TaskCompletionSource<IResult> TaskCompletion { get; }

        public IntroductionOverlayViewModel(int slidesCount = -1)
        {
            ServiceProvider = DI.Default;
            SlidesCount = slidesCount;
            CurrentStep = $"1/{slidesCount}";
            FileSystems = new();
            TaskCompletion = new();
        }

        /// <inheritdoc/>
        public async Task InitAsync(CancellationToken cancellationToken = default)
        {
            var installations = await VaultFileSystemService.GetFileSystemInstallationsAsync(cancellationToken).ToArrayAsyncImpl(cancellationToken);
            await foreach (var item in VaultFileSystemService.GetFileSystemsAsync(cancellationToken))
            {
                var status = await item.GetStatusAsync(cancellationToken);
                if (status == FileSystemAvailability.Available)
                {
                    FileSystems.Add(new FileSystemItemViewModel(item)
                    {
                        IsDefault = item.Id == Constants.DataSources.DEFAULT_FILE_SYSTEM
                    });
                }
                else
                {
                    var installation = installations.FirstOrDefault(x => x.Id == item.Id);
                    if (installation is not null)
                        FileSystems.Add(installation);
                }
            }
        }

        public bool Next()
        {
            if (CurrentIndex >= SlidesCount - 1)
                return false;

            CurrentIndex++;
            return true;
        }

        public bool Previous()
        {
            if (CurrentIndex <= 0)
                return false;

            CurrentIndex--;
            return true;
        }

        [RelayCommand]
        private async Task OpenSettingsAsync()
        {
            await OverlayService.ShowAsync(SettingsOverlayViewModel.Instance);
            await SettingsService.TrySaveAsync();
        }

        partial void OnCurrentIndexChanged(int value)
        {
            CurrentStep = $"{CurrentIndex+1}/{SlidesCount}";
        }
    }
}
