using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SecureFolderFS.Sdk.AppModels.Sorters;
using SecureFolderFS.Sdk.Enums;
using SecureFolderFS.Sdk.Extensions;
using SecureFolderFS.Sdk.ViewModels.Controls.Components;
using SecureFolderFS.Sdk.ViewModels.Controls.Storage.Browser;
using SecureFolderFS.Shared.ComponentModel;

namespace SecureFolderFS.Sdk.ViewModels.Controls.Storage
{
    [Bindable(true)]
    public sealed partial class LayoutsViewModel : ObservableObject, IViewable
    {
        [ObservableProperty] private ObservableCollection<PickerOptionViewModel> _SortOptions;
        [ObservableProperty] private ObservableCollection<PickerOptionViewModel> _SizeOptions;
        [ObservableProperty] private ObservableCollection<PickerOptionViewModel> _ViewOptions;
        [ObservableProperty] private PickerOptionViewModel? _CurrentSortOption;
        [ObservableProperty] private PickerOptionViewModel? _CurrentSizeOption;
        [ObservableProperty] private PickerOptionViewModel? _CurrentViewOption;
        [ObservableProperty] private BrowserViewType _BrowserViewType;
        [ObservableProperty] private bool _AreSizeOptionsAvailable;
        [ObservableProperty] private bool _IsAscending;
        [ObservableProperty] private string? _Title;

        public LayoutsViewModel()
        {
            SortOptions = new([
                new(nameof(NameSorter), "Name".ToLocalized()),
                new(nameof(KindSorter), "Kind".ToLocalized()),
                new(nameof(SizeSorter), "Size".ToLocalized())
            ]);
            SizeOptions = new([
                new("Small", "SmallSize".ToLocalized()),
                new("Medium", "MediumSize".ToLocalized()),
                new("Large", "LargeSize".ToLocalized())
            ]);
            ViewOptions = new([
                new(nameof(BrowserViewType.ListView), "ListViewLayout".ToLocalized()),
                new(nameof(BrowserViewType.ColumnView), "ColumnViewLayout".ToLocalized()),
                new("GridView", "GridViewLayout".ToLocalized()),
                new("GalleryView", "GalleryViewLayout".ToLocalized())
            ]);

            IsAscending = true;
            CurrentSortOption = SortOptions[0];
            CurrentSizeOption = SizeOptions[1];
            CurrentViewOption = ViewOptions[0];
            Title = "ViewOptions".ToLocalized();
        }

        public IItemSorter<BrowserItemViewModel> GetSorter()
        {
            return CurrentSortOption?.Id switch
            {
                nameof(NameSorter) => IsAscending ? NameSorter.Ascending : NameSorter.Descending,
                nameof(KindSorter) => IsAscending ? KindSorter.Ascending : KindSorter.Descending,
                nameof(SizeSorter) => IsAscending ? SizeSorter.Ascending : SizeSorter.Descending,
                _ => NameSorter.Descending
            };
        }

        partial void OnCurrentViewOptionChanged(PickerOptionViewModel? value)
        {
            switch (value?.Id)
            {
                case nameof(BrowserViewType.ListView):
                    BrowserViewType = BrowserViewType.ListView;
                    break;

                case nameof(BrowserViewType.ColumnView):
                    BrowserViewType = BrowserViewType.ColumnView;
                    break;
            }

            UpdateLayoutSizeOption(CurrentSizeOption);
        }

        partial void OnCurrentSizeOptionChanged(PickerOptionViewModel? value)
        {
            UpdateLayoutSizeOption(value);
        }

        [RelayCommand]
        private void SetLayoutSizeOption(object? value)
        {
            CurrentSizeOption = value switch
            {
                int index => SizeOptions.ElementAtOrDefault(index),
                string strValue => strValue.ToLowerInvariant() switch
                {
                    "small" => SizeOptions[0],
                    "medium" => SizeOptions[1],
                    "large" => SizeOptions[2],
                    _ => CurrentSizeOption
                },
                _ => CurrentSizeOption
            };
        }

        private bool UpdateLayoutSizeOption(PickerOptionViewModel? value)
        {
            switch (CurrentViewOption?.Id)
            {
                case "GridView":
                    AreSizeOptionsAvailable = true;
                    BrowserViewType = value?.Id switch
                    {
                        "Small" => BrowserViewType.SmallGridView,
                        "Medium" => BrowserViewType.MediumGridView,
                        "Large" => BrowserViewType.LargeGridView,
                        _ => BrowserViewType
                    };
                    return true;

                case "GalleryView":
                    AreSizeOptionsAvailable = true;
                    BrowserViewType = value?.Id switch
                    {
                        "Small" => BrowserViewType.SmallGalleryView,
                        "Medium" => BrowserViewType.MediumGalleryView,
                        "Large" => BrowserViewType.LargeGalleryView,
                        _ => BrowserViewType
                    };
                    return true;

                default:
                        AreSizeOptionsAvailable = false;
                    return false;
            }
        }
    }
}