﻿using System;
using CommunityToolkit.Mvvm.ComponentModel;
using System.ComponentModel;

namespace SecureFolderFS.Sdk.ViewModels.Views.Overlays
{
    /// <summary>
    /// Serves as the base dialog view model containing reusable code for every dialog.
    /// </summary>
    [Bindable(true)]
    [Obsolete("Use BaseDesignationViewModel and IOverlayControls.")]
    public abstract partial class OverlayViewModel : BaseDesignationViewModel
    {
        /// <summary>
        /// Gets or sets whether the primary button should be enabled or not.
        /// </summary>
        [ObservableProperty] private bool _PrimaryButtonEnabled;

        /// <summary>
        /// Gets or sets whether the secondary button should be enabled or not.
        /// </summary>
        [ObservableProperty] private bool _SecondaryButtonEnabled;

        /// <summary>
        /// Gets or sets the text of primary button. If value is null, the button is hidden.
        /// </summary>
        [ObservableProperty] private string? _PrimaryButtonText;

        /// <summary>
        /// Gets or sets the text of secondary button. If value is null, the button is hidden.
        /// </summary>
        [ObservableProperty] private string? _SecondaryButtonText;

        /// <summary>
        /// Gets or sets the text of close button. If value is null, the button is hidden.
        /// </summary>
        [ObservableProperty] private string? _CloseButtonText;
    }
}
