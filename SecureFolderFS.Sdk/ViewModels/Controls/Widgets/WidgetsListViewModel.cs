using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using SecureFolderFS.Shared.Extensions;
using SecureFolderFS.Shared.Utils;

namespace SecureFolderFS.Sdk.ViewModels.Controls.Widgets
{
    public sealed class WidgetsListViewModel : ObservableObject, IAsyncInitialize
    {
        public ObservableCollection<BaseWidgetViewModel> Widgets { get; }

        // TODO: Remove later in favor of WidgetsListViewModel.Widgets
        public VaultHealthWidgetViewModel? HealthWidget { get; private set; }

        public GraphsWidgetViewModel? GraphsWidget { get; private set; }

        public WidgetsListViewModel()
        {
            Widgets = new();
        }

        public async Task InitAsync(CancellationToken cancellationToken = default)
        {
            //await Task.WhenAll(Widgets.Select(x => x.InitAsync(cancellationToken)));

            // TODO: Load widgets from config

            HealthWidget = new();
            GraphsWidget = new();

            await Task.WhenAll(AsyncExtensions.CombineAsyncInitialize(cancellationToken, GraphsWidget, HealthWidget));
        }
    }
}
