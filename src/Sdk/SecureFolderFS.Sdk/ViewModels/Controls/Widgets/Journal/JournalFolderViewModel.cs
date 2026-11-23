using System.ComponentModel;
using System.IO;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using SecureFolderFS.Sdk.Extensions;
using SecureFolderFS.Shared.ComponentModel;

namespace SecureFolderFS.Sdk.ViewModels.Controls.Widgets.Journal
{
    /// <summary>
    /// Represents a journal folder where entries are stored.
    /// </summary>
    [Bindable(true)]
    public sealed partial class JournalFolderViewModel : ObservableObject, IViewable
    {
        [ObservableProperty] private string? _Title;

        /// <summary>
        /// Gets the folder relative to the vault root, or null when this item stands for all journal folders.
        /// </summary>
        public string? RelativePath { get; }

        /// <summary>
        /// Gets the command invoked to stop keeping entries in this folder.
        /// </summary>
        public ICommand? RemoveCommand { get; init; }

        public JournalFolderViewModel(string? relativePath)
        {
            RelativePath = relativePath;
            Title = relativePath is null
                ? "AllJournals".ToLocalized()
                : Path.GetFileName(relativePath.TrimEnd('/'));
        }
    }
}
