using System.ComponentModel;
using SecureFolderFS.Shared.ComponentModel;

namespace SecureFolderFS.Sdk.ViewModels.Controls.Components
{
    /// <summary>
    /// Represents a picker option with a <see cref="IViewable.Title"/> property and a unique ID.
    /// </summary>
    [Bindable(true)]
    public partial class PickerOptionViewModel : SelectableItemViewModel
    {
        /// <summary>
        /// Gets the unique ID associated with this option.
        /// </summary>
        public virtual string Id { get; }

        public PickerOptionViewModel(string id, string? title = null)
        {
            Id = id;
            Title = title;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return Title ?? string.Empty;
        }
    }
}
