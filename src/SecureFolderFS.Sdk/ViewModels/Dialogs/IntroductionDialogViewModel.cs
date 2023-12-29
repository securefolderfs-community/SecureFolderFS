using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using SecureFolderFS.Sdk.Attributes;
using SecureFolderFS.Sdk.Extensions;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Shared.Extensions;
using SecureFolderFS.Shared.ComponentModel;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.ViewModels.Dialogs
{
    [Inject<IDialogService>, Inject<ISettingsService>]
    public sealed partial class IntroductionDialogViewModel : ObservableObject
    {
        private readonly int _maxAmount;

        [ObservableProperty] private int _CurrentIndex;
        [ObservableProperty] private string? _CurrentStep;

        public TaskCompletionSource<IResult> TaskCompletion { get; }

        public IntroductionDialogViewModel(int maxAmount)
        {
            ServiceProvider = Ioc.Default;
            _maxAmount = maxAmount;
            CurrentStep = $"1/{maxAmount}";
            TaskCompletion = new();
        }

        [RelayCommand]
        private async Task OpenSettingsAsync()
        {
            await DialogService.ShowDialogAsync(SettingsDialogViewModel.Instance);
            await SettingsService.TrySaveAsync();
        }

        partial void OnCurrentIndexChanged(int value)
        {
            CurrentStep = $"{CurrentIndex+1}/{_maxAmount}";
        }
    }
}
