using System;
using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace SecureFolderFS.Sdk.ViewModels.Views.Overlays
{
    /// <summary>
    /// Serves as the base dialog view model containing reusable code for every dialog.
    /// </summary>
    [Bindable(true)]
    [Obsolete("Use BaseDesignationViewModel and IOverlayControls.")]
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

        /// <summary>
        /// Gets or sets the text of close button. If value is null, the button is hidden.
        /// </summary>
        [Obsolete("Use SecondaryText property instead.")]
        [ObservableProperty] private string? _CloseButtonText;
    }
}
