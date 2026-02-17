using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SecureFolderFS.Sdk.Attributes;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Shared;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Shared.Extensions;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using SecureFolderFS.Sdk.ViewModels.Controls;
using SecureFolderFS.Sdk.ViewModels.Controls.Components;

namespace SecureFolderFS.Sdk.ViewModels.Views.Overlays
{
    [Inject<IOverlayService>, Inject<ISettingsService>]
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
            TaskCompletion = new();
        }

        /// <inheritdoc/>
        public Task InitAsync(CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
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
