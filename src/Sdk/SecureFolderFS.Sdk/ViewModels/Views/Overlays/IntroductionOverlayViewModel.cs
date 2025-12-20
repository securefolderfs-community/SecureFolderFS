using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SecureFolderFS.Sdk.Attributes;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Shared;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Shared.Extensions;
using System.ComponentModel;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.ViewModels.Views.Overlays
{
    [Inject<IOverlayService>, Inject<ISettingsService>]
    [Bindable(true)]
    public sealed partial class IntroductionOverlayViewModel : OverlayViewModel
    {
        [ObservableProperty] private int _CurrentIndex;
        [ObservableProperty] private string? _CurrentStep;

        public int SlidesCount { get; set; }

        public TaskCompletionSource<IResult> TaskCompletion { get; }

        public IntroductionOverlayViewModel(int slidesCount = -1)
        {
            ServiceProvider = DI.Default;
            SlidesCount = slidesCount;
            CurrentStep = $"1/{slidesCount}";
            TaskCompletion = new();
        }

        public void Next()
        {
            if (CurrentIndex < SlidesCount - 1)
                CurrentIndex++;
        }

        public void Previous()
        {
            if (CurrentIndex > 0)
                CurrentIndex--;
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
