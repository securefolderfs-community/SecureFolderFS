using CommunityToolkit.Mvvm.ComponentModel;
using SecureFolderFS.Shared.ComponentModel;
using System.ComponentModel;

namespace SecureFolderFS.Sdk.ViewModels
{
    /// <summary>
    /// Represents a vault option.
    /// </summary>
    [Bindable(true)]
    public sealed class VaultOptionViewModel(string id, string? title = null) : ObservableObject, IViewable
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
