using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace SecureFolderFS.Sdk.ViewModels.Dialogs
{
    /// <summary>
    /// Serves as the base dialog view model containing reusable,
    /// and optional boilerplate code for every dialog.
    /// </summary>
    public abstract partial class DialogViewModel : ObservableObject
    {
        /// <summary>
        /// Gets or sets the title of the dialog.
        /// </summary>
        [ObservableProperty]
        private string? _Title;

        /// <summary>
        /// Gets or sets whether the primary button should be enabled or not.
        /// </summary>
        [ObservableProperty]
        private bool _PrimaryButtonEnabled;

        /// <summary>
        /// Gets or sets whether the secondary button should be enabled or not.
        /// </summary>
        [ObservableProperty]
        private bool _SecondaryButtonEnabled;

        /// <summary>
        /// Gets or sets the text of primary button. If value is null, the button is hidden.
        /// </summary>
        [ObservableProperty]
        private string? _PrimaryButtonText;

        /// <summary>
        /// Gets or sets the text of secondary button. If value is null, the button is hidden.
        /// </summary>
        [ObservableProperty]
        private string? _SecondaryButtonText;

        /// <summary>
        /// Gets or sets the text of close button. If value is null, the button is hidden.
        /// </summary>
        [ObservableProperty]
        private string? _CloseButtonText;
    }
}
