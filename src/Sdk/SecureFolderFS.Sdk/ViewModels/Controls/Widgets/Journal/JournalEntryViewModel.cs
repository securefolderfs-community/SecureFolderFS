using System.ComponentModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using SecureFolderFS.Sdk.AppModels;
using SecureFolderFS.Sdk.Attributes;
using SecureFolderFS.Sdk.Extensions;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Shared;
using SecureFolderFS.Shared.ComponentModel;

namespace SecureFolderFS.Sdk.ViewModels.Controls.Widgets.Journal
{
    /// <summary>
    /// Represents a journal entry in a list of entries.
    /// </summary>
    [Inject<ILocalizationService>]
    [Bindable(true)]
    public sealed partial class JournalEntryViewModel : ObservableObject, IViewable
    {
        [ObservableProperty] private string? _Title;
        [ObservableProperty] private string? _Snippet;

        /// <summary>
        /// Gets or sets when the entry was last saved, relative to now.
        /// </summary>
        [ObservableProperty] private string? _SavedText;

        /// <summary>
        /// Gets or sets the day of the month the entry belongs to.
        /// </summary>
        [ObservableProperty] private string? _DayText;

        /// <summary>
        /// Gets or sets the abbreviated month the entry belongs to.
        /// </summary>
        [ObservableProperty] private string? _MonthText;

        /// <summary>
        /// Gets or sets whether this entry is the one open in the editor.
        /// </summary>
        [ObservableProperty] private bool _IsSelected;

        /// <summary>
        /// Gets the entry this item represents.
        /// </summary>
        public JournalEntry Entry { get; private set; }

        /// <summary>
        /// Gets the command invoked to open this entry.
        /// </summary>
        public ICommand? OpenCommand { get; init; }

        public JournalEntryViewModel(JournalEntry entry)
        {
            ServiceProvider = DI.Default;
            Entry = entry;
            Update(entry);
        }

        /// <summary>
        /// Refreshes this item from a newer version of its entry, e.g. after it has been saved.
        /// </summary>
        /// <param name="entry">The entry to show.</param>
        public void Update(JournalEntry entry)
        {
            Entry = entry;
            Title = entry.Title.Length > 0 ? entry.Title : "UntitledEntry".ToLocalized();
            Snippet = entry.Snippet;
            SavedText = LocalizationService.LocalizeDate(entry.Modified);
            DayText = entry.Date.Day.ToString(LocalizationService.CurrentCulture);
            MonthText = entry.Date.ToString("MMM", LocalizationService.CurrentCulture).ToUpper(LocalizationService.CurrentCulture);
        }
    }
}
