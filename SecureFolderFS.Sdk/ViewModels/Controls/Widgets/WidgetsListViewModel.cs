using System;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Shared.Utils;

namespace SecureFolderFS.Sdk.ViewModels.Controls.Widgets
{
    public sealed class WidgetsListViewModel : ObservableObject, IAsyncInitialize
    {
        private IWidgetsContextModel WidgetsContextModel { get; }

        public ObservableCollection<BaseWidgetViewModel> Widgets { get; }

        public WidgetsListViewModel(IWidgetsContextModel widgetsContextModel)
        {
            WidgetsContextModel = widgetsContextModel;
            Widgets = new();
        }

        /// <inheritdoc/>
        public async Task InitAsync(CancellationToken cancellationToken = default)
        {
            await foreach (var item in WidgetsContextModel.GetWidgetsAsync(cancellationToken))
            {
                var widgetViewModel = GetWidgetForModel(item);
                _ = widgetViewModel.InitAsync(cancellationToken);

                Widgets.Add(widgetViewModel);
            }
        }

        private BaseWidgetViewModel GetWidgetForModel(IWidgetModel widgetModel)
        {
            switch (widgetModel.WidgetId)
            {
                case Constants.Widgets.HEALTH_WIDGET_ID:
                    return new VaultHealthWidgetViewModel(widgetModel);

                case Constants.Widgets.GRAPHS_WIDGET_ID:
                    return new GraphsWidgetViewModel(widgetModel);

                default:
                    throw new ArgumentOutOfRangeException(nameof(widgetModel.WidgetId));
            }
        }
    }
}
