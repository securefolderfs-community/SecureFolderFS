using CommunityToolkit.Mvvm.ComponentModel;
using System.ComponentModel;

namespace SecureFolderFS.Sdk.ViewModels.Views.Overlays
{
    /// <summary>
    /// Serves as the base dialog view model containing reusable code for every dialog.
    /// </summary>
    [Bindable(true)]
    public abstract partial class OverlayViewModel : BaseDesignationViewModel, IOverlayControls
    {
        /// <inheritdoc cref="IOverlayControls.CanContinue"/>
        [ObservableProperty] private bool _CanContinue;

        /// <inheritdoc cref="IOverlayControls.CanCancel"/>
        [ObservableProperty] private bool _CanCancel;

        /// <inheritdoc cref="IOverlayControls.PrimaryText"/>
        [ObservableProperty] private string? _PrimaryText;

        /// <inheritdoc cref="IOverlayControls.SecondaryText"/>
        [ObservableProperty] private string? _SecondaryText;
    }
}
