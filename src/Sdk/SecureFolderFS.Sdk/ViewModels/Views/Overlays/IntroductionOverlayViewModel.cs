using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SecureFolderFS.Sdk.Attributes;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.ViewModels.Controls.Components;
using SecureFolderFS.Shared;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Shared.Extensions;
using SecureFolderFS.Storage.Enums;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.ViewModels.Views.Overlays
{
    [Inject<IOverlayService>, Inject<ISettingsService>, Inject<IVaultFileSystemService>, Inject<IMediaService>]
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
            var items = new List<PickerOptionViewModel>();

            await foreach (var item in VaultFileSystemService.GetFileSystemsAsync(cancellationToken))
            {
                var status = await item.GetStatusAsync(cancellationToken);
                if (status == FileSystemAvailability.Available)
                {
                    items.Add(new FileSystemItemViewModel(item)
                    {
                        Icon = await MediaService.GetImageFromResourceAsync(item.Id, cancellationToken),
                        IsDefault = item.Id == Constants.DataSources.DEFAULT_FILE_SYSTEM
                    });
                }
                else
                {
                    var installation = installations.FirstOrDefault(x => x.Id == item.Id);
                    if (installation is not null)
                        items.Add(installation);
                }
            }

            foreach (var sortedItem in items.OrderByDescending(x => x is FileSystemItemViewModel))
                FileSystems.Add(sortedItem);
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
