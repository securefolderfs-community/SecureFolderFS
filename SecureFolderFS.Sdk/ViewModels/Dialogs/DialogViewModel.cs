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
        private bool _PrimaryButtonEnabled;
        private bool _SecondaryButtonEnabled;
        private string? _PrimaryButtonText;
        private string? _SecondaryButtonText;
        private string? _CloseButtonText;

        /// <summary>
        /// Gets or sets the title of the dialog.
        /// </summary>
        public string? Title
        {
            get => _Title;
            set => SetProperty(ref _Title, value);
        }

        /// <summary>
        /// Gets or sets whether the primary button should be enabled or not.
        /// </summary>
        public bool PrimaryButtonEnabled
        {
            get => _PrimaryButtonEnabled;
            set => SetProperty(ref _PrimaryButtonEnabled, value);
        }

        /// <summary>
        /// Gets or sets whether the secondary button should be enabled or not.
        /// </summary>
        public bool SecondaryButtonEnabled
        {
            get => _SecondaryButtonEnabled;
            set => SetProperty(ref _SecondaryButtonEnabled, value);
        }

        /// <summary>
        /// Gets or sets the text of primary button. If value is null, the button is hidden.
        /// </summary>
        public string? PrimaryButtonText
        {
            get => _PrimaryButtonText;
            set => SetProperty(ref _PrimaryButtonText, value);
        }

        /// <summary>
        /// Gets or sets the text of secondary button. If value is null, the button is hidden.
        /// </summary>
        public string? SecondaryButtonText
        {
            get => _SecondaryButtonText;
            set => SetProperty(ref _SecondaryButtonText, value);
        }

        /// <summary>
        /// Gets or sets the text of close button. If value is null, the button is hidden.
        /// </summary>
        public string? CloseButtonText
        {
            get => _CloseButtonText;
            set => SetProperty(ref _CloseButtonText, value);
        }

        /// <summary>
        /// The relay command executed on primary button click.
        /// </summary>
        public IRelayCommand? PrimaryButtonClickCommand { get; protected init; }

        /// <summary>
        /// The relay command executed on secondary button click.
        /// </summary>
        public IRelayCommand? SecondaryButtonClickCommand { get; protected init; }

        /// <summary>
        /// The relay command executed on close button click.
        /// </summary>
        public IRelayCommand? CloseButtonClickCommand { get; protected init; }
    }
}
