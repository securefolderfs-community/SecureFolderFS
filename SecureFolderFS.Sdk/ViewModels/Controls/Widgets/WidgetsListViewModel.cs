using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace SecureFolderFS.Sdk.ViewModels.Controls.Widgets
{
    public sealed class WidgetsListViewModel : ObservableObject
    {
        public ObservableCollection<BaseWidgetViewModel> Widgets { get; }

        public WidgetsListViewModel()
        {
            Widgets = new();
        }
    }
}
