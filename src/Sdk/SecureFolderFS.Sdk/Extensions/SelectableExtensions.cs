using System.Collections.Generic;
using System.Linq;
using SecureFolderFS.Sdk.ViewModels.Controls.Components;

namespace SecureFolderFS.Sdk.Extensions
{
    public static class SelectableExtensions
    {
        public static void SelectAll<T>(this ICollection<T> collection)
            where T : SelectableItemViewModel
        {
            foreach (var item in collection)
                item.IsSelected = true;
        }

        public static void UnselectAll<T>(this ICollection<T> collection)
            where T : SelectableItemViewModel
        {
            foreach (var item in collection)
                item.IsSelected = false;
        }

        public static IEnumerable<T> GetSelectedItems<T>(this ICollection<T> collection)
            where T : SelectableItemViewModel
        {
            return collection.Where(item => item.IsSelected);
        }
    }
}