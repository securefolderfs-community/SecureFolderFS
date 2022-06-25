using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace SecureFolderFS.Sdk.ViewModels.Dialogs
{
    /// <summary>
    /// Serves as the base dialog view model containing reusable,
    /// and optional boilerplate code for every dialog.
    /// </summary>
    public abstract class DialogViewModel : ObservableObject
    {
        private string? _Title;
        public string? Title
        {
            get => _Title;
            set => SetProperty(ref _Title, value);
        }

        private bool _PrimaryButtonEnabled;
        public bool PrimaryButtonEnabled
        {
            get => _PrimaryButtonEnabled;
            set => SetProperty(ref _PrimaryButtonEnabled, value);
        }

        private bool _SecondaryButtonEnabled;
        public bool SecondaryButtonEnabled
        {
            get => _SecondaryButtonEnabled;
            set => SetProperty(ref _SecondaryButtonEnabled, value);
        }

        private string? _PrimaryButtonText;
        public string? PrimaryButtonText
        {
            get => _PrimaryButtonText;
            set => SetProperty(ref _PrimaryButtonText, value);
        }

        private string? _SecondaryButtonText;
        public string? SecondaryButtonText
        {
            get => _SecondaryButtonText;
            set => SetProperty(ref _SecondaryButtonText, value);
        }

        private string? _CloseButtonText;
        public string? CloseButtonText
        {
            get => _CloseButtonText;
            set => SetProperty(ref _CloseButtonText, value);
        }

        public IRelayCommand? PrimaryButtonClickCommand { get; protected init; }

        public IRelayCommand? SecondaryButtonClickCommand { get; protected init; }

        public IRelayCommand? CloseButtonClickCommand { get; protected init; }
    }
}
