﻿using CommunityToolkit.Mvvm.ComponentModel;
using SecureFolderFS.Shared.ComponentModel;
using System.ComponentModel;

namespace SecureFolderFS.Sdk.ViewModels.Views
{
    /// <summary>
    /// Represents a base view model for views that intend to implement <see cref="IViewDesignation"/>.
    /// </summary>
    [Bindable(true)]
    public abstract partial class BaseDesignationViewModel : ObservableObject, IViewDesignation
    {
        /// <inheritdoc cref="IViewable.Title"/>
        [ObservableProperty] private string? _Title;

        /// <inheritdoc/>
        public virtual void OnAppearing()
        {
        }

        /// <inheritdoc/>
        public virtual void OnDisappearing()
        {
        }
    }
}
