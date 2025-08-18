using CommunityToolkit.Mvvm.ComponentModel;
using SecureFolderFS.Shared.ComponentModel;
using System.ComponentModel;

namespace SecureFolderFS.Sdk.ViewModels.Controls
{
    /// <summary>
    /// Represents a picker option with a <see cref="IViewable.Title"/> property and a unique ID.
    /// </summary>
    [Bindable(true)]
    public sealed class PickerOptionViewModel(string id, string? title = null) : ObservableObject, IViewable
    {
        /// <summary>
        /// Gets the unique ID associated with this option.
        /// </summary>
        public string Id { get; } = id;

        /// <inheritdoc/>
        public string Title { get; } = title ?? id;

        /// <inheritdoc/>
        public override string ToString()
        {
            return Title;
        }
    }
}
