using CommunityToolkit.Mvvm.ComponentModel;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Shared.Extensions;
using SecureFolderFS.Shared.Utilities;
using System;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.ViewModels.Controls.Widgets
{
    public sealed class WidgetsListViewModel : ObservableObject, IAsyncInitialize, IDisposable
    {
        private readonly IWidgetsCollectionModel _widgetsContextModel;
        private readonly IVaultLifetimeModel _vaultLifeTimeModel;

        public ObservableCollection<BaseWidgetViewModel> Widgets { get; }

        public WidgetsListViewModel(IVaultLifetimeModel vaultLifeTimeModel, IWidgetsCollectionModel widgetsContextModel)
        {
            _vaultLifeTimeModel = vaultLifeTimeModel;
            _widgetsContextModel = widgetsContextModel;
            Widgets = new();
        }

        /// <inheritdoc/>
        public async Task InitAsync(CancellationToken cancellationToken = default)
        {
            // Load widgets for vault
            if (!await _widgetsContextModel.TryLoadAsync(cancellationToken))
                return;

            // Add widgets
            foreach (var item in _widgetsContextModel.GetWidgets())
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
                    return new GraphsWidgetViewModel(_vaultLifeTimeModel.VaultStatisticsModel, widgetModel);

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
