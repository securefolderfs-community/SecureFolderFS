using CommunityToolkit.Mvvm.ComponentModel;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Shared.Extensions;
using SecureFolderFS.Storage.VirtualFileSystem;
using System;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.ViewModels.Controls.Widgets
{
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
                    return new VaultHealthWidgetViewModel(widgetModel);

                case Constants.Widgets.GRAPHS_WIDGET_ID:
                {
                    return _unlockedVaultViewModel.StorageRoot is IVFSRootFolder { ReadWriteStatistics: { } statistics }
                        ? new GraphsWidgetViewModel(statistics, widgetModel)
                        : null;
                }

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
