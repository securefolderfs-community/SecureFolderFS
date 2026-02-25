using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using SecureFolderFS.Shared.ComponentModel;

namespace SecureFolderFS.Sdk.ViewModels.Controls.Components
{
    [Bindable(true)]
    public abstract partial class SelectableItemViewModel : ObservableObject, IViewable
    {
        /// <inheritdoc cref="IViewable.Title"/>
        [ObservableProperty] private string? _Title;

        /// <summary>
        /// Gets or sets the value indicating whether the item is selected in view.
        /// </summary>
        [ObservableProperty] private bool _IsSelected;

        /// <summary>
        /// Called when <see cref="IsSelected"/> changes.
        /// </summary>
        protected virtual void IsSelectedChanged(bool newValue)
        {
        }

        partial void OnIsSelectedChanged(bool value)
        {
            IsSelectedChanged(value);
        }
    }
}
