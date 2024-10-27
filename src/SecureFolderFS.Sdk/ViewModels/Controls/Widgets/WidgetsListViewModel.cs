using CommunityToolkit.Mvvm.ComponentModel;
using SecureFolderFS.Sdk.AppModels;
using SecureFolderFS.Sdk.Extensions;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.ViewModels.Controls.Widgets.Categories;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Shared.Extensions;
using SecureFolderFS.Storage.Scanners;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.ViewModels.Controls.Widgets
{
    [Bindable(true)]
    public sealed class WidgetsListViewModel : ObservableObject, IAsyncInitialize, IDisposable
    {
        private readonly UnlockedVaultViewModel _unlockedVaultViewModel;

        public IWidgetsCollectionModel WidgetsCollectionModel { get; }

        public ObservableCollection<BaseWidgetViewModel> Widgets { get; }

        public WidgetsListViewModel(UnlockedVaultViewModel unlockedVaultViewModel, IWidgetsCollectionModel widgetsCollectionModel)
        {
            _unlockedVaultViewModel = unlockedVaultViewModel;
            WidgetsCollectionModel = widgetsCollectionModel;
            Widgets = new();
        }

        /// <inheritdoc/>
        public async Task InitAsync(CancellationToken cancellationToken = default)
        {
            // Load widgets for vault
            if (!await WidgetsCollectionModel.TryLoadAsync(cancellationToken))
                return;

            // Add widgets
            foreach (var item in WidgetsCollectionModel.GetWidgets())
            {
                var widgetViewModel = GetWidgetForModel(item);
                if (widgetViewModel is null)
                    continue;

                _ = widgetViewModel.InitAsync(cancellationToken);
                Widgets.Add(widgetViewModel);
            }
        }

        private BaseWidgetViewModel? GetWidgetForModel(IWidgetModel widgetModel)
        {
            switch (widgetModel.WidgetId)
            {
                case Constants.Widgets.HEALTH_WIDGET_ID:
                    return new HealthWidgetViewModel(_unlockedVaultViewModel, widgetModel);

                case Constants.Widgets.GRAPHS_WIDGET_ID:
                    return new GraphsWidgetViewModel(_unlockedVaultViewModel, widgetModel);
                
                case Constants.Widgets.AGGREGATED_DATA_WIDGET_ID:
                    return new AggregatedDataWidgetViewModel(_unlockedVaultViewModel, widgetModel);

                default:
                    return null;
            }
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            foreach (var item in Widgets)
                item.Dispose();
        }
    }
}
