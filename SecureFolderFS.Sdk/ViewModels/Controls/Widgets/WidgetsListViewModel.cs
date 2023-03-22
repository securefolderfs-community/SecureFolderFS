using CommunityToolkit.Mvvm.ComponentModel;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Shared.Utils;
using System;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.ViewModels.Controls.Widgets
{
    public sealed class WidgetsListViewModel : ObservableObject, IAsyncInitialize, IDisposable
    {
        private readonly IWidgetsCollectionModel _widgetsContextModel;
        private readonly IUnlockedVaultModel _unlockedVaultModel;

        public ObservableCollection<BaseWidgetViewModel> Widgets { get; }

        public WidgetsListViewModel(IUnlockedVaultModel unlockedVaultModel, IWidgetsCollectionModel widgetsContextModel)
        {
            _unlockedVaultModel = unlockedVaultModel;
            _widgetsContextModel = widgetsContextModel;
            Widgets = new();
        }

        /// <inheritdoc/>
        public Task InitAsync(CancellationToken cancellationToken = default)
        {
            foreach (var item in _widgetsContextModel.GetWidgets())
            {
                var widgetViewModel = GetWidgetForModel(item);
                _ = widgetViewModel.InitAsync(cancellationToken);

                Widgets.Add(widgetViewModel);
            }

            return Task.CompletedTask;
        }

        private BaseWidgetViewModel GetWidgetForModel(IWidgetModel widgetModel)
        {
            switch (widgetModel.WidgetId)
            {
                case Constants.Widgets.HEALTH_WIDGET_ID:
                    return new VaultHealthWidgetViewModel(widgetModel);

                case Constants.Widgets.GRAPHS_WIDGET_ID:
                    return new GraphsWidgetViewModel(_unlockedVaultModel.VaultStatisticsModel, widgetModel);

                default:
                    throw new ArgumentOutOfRangeException(nameof(widgetModel.WidgetId));
            }
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            foreach (var item in Widgets)
            {
                item.Dispose();
            }
        }
    }
}
