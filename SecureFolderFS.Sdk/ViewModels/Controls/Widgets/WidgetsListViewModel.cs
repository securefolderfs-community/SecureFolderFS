using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace SecureFolderFS.Sdk.ViewModels.Controls.Widgets
{
    public sealed class WidgetsListViewModel : ObservableObject
    {
        public ObservableCollection<BaseWidgetViewModel> Widgets { get; }

        // TODO: Remove later in favor of WidgetsListViewModel.Widgets
        public VaultHealthWidgetViewModel HealthWidget { get; }

        public GraphsWidgetViewModel GraphsWidget { get; }

        public WidgetsListViewModel()
        {
            Widgets = new();
            HealthWidget = new();
            GraphsWidget = new();
        }
    }
}
