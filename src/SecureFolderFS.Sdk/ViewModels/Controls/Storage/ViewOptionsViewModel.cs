using System.Collections.ObjectModel;
using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using SecureFolderFS.Sdk.AppModels.Sorters;
using SecureFolderFS.Sdk.Extensions;
using SecureFolderFS.Sdk.ViewModels.Controls.Storage.Browser;
using SecureFolderFS.Shared.ComponentModel;

namespace SecureFolderFS.Sdk.ViewModels.Controls.Storage
{
    [Bindable(true)]
    public sealed partial class ViewOptionsViewModel : ObservableObject, IViewable
    {
        [ObservableProperty] private ObservableCollection<PickerOptionViewModel> _SortOptions;
        [ObservableProperty] private PickerOptionViewModel? _CurrentSortOption;
        [ObservableProperty] private bool _IsAscending;
        [ObservableProperty] private string? _Title;

        public ViewOptionsViewModel()
        {
            SortOptions = new([
                new(nameof(NameSorter), "Name".ToLocalized()),
                new(nameof(KindSorter), "Kind".ToLocalized()),
                new(nameof(SizeSorter), "Size".ToLocalized())
            ]);
            CurrentSortOption = SortOptions[0];
            Title = "ViewOptions".ToLocalized();
        }

        public IItemSorter<BrowserItemViewModel>? GetSorter()
        {
            return CurrentSortOption?.Id switch
            {
                nameof(NameSorter) => IsAscending ? NameSorter.Ascending : NameSorter.Descending,
                nameof(KindSorter) => IsAscending ? KindSorter.Ascending : KindSorter.Descending,
                nameof(SizeSorter) => IsAscending ? SizeSorter.Ascending : SizeSorter.Descending,
                _ => null
            };
        }
    }
}