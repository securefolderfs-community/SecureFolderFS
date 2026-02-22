using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using SecureFolderFS.Sdk.Helpers;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.ViewModels.Controls.Widgets.Data;
using SecureFolderFS.Sdk.ViewModels.Controls.Widgets.Health;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Shared.Extensions;

namespace SecureFolderFS.Sdk.ViewModels.Controls.Widgets
{
    [Bindable(true)]
    public sealed class WidgetsListViewModel : ObservableObject, IAsyncInitialize, IDisposable
    {
        private readonly INavigator _dashboardNavigator;
        private readonly UnlockedVaultViewModel _unlockedVaultViewModel;
        private readonly SynchronizationContext? _synchronizationContext;
        private readonly CollectionReorderHelper<BaseWidgetViewModel> _reorderHelper;

        public IWidgetsCollectionModel WidgetsCollectionModel { get; }

        public ObservableCollection<BaseWidgetViewModel> Widgets { get; }

        public WidgetsListViewModel(UnlockedVaultViewModel unlockedVaultViewModel, INavigator dashboardNavigator, IWidgetsCollectionModel widgetsCollectionModel)
        {
            _dashboardNavigator = dashboardNavigator;
            _unlockedVaultViewModel = unlockedVaultViewModel;
            _synchronizationContext = SynchronizationContext.Current;
            _reorderHelper = new();
            WidgetsCollectionModel = widgetsCollectionModel;
            Widgets = new();

            // Subscribe to collection changes to persist reordering
            Widgets.CollectionChanged += Widgets_CollectionChanged;
            _reorderHelper.Reordered += ReorderHelper_Reordered;
        }

        /// <inheritdoc/>
        public async Task InitAsync(CancellationToken cancellationToken = default)
        {
            await _synchronizationContext.PostOrExecuteAsync(async state =>
            {
                // Load widgets for vault
                if (!await WidgetsCollectionModel.TryInitAsync(cancellationToken))
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
            });
        }

        private void Widgets_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                {
                    if (e.NewItems?.Cast<BaseWidgetViewModel>().FirstOrDefault() is { } widget)
                        _reorderHelper.RegisterAdd(widget);

                    break;
                }

                case NotifyCollectionChangedAction.Remove:
                {
                    if (e.OldItems?.Cast<BaseWidgetViewModel>().FirstOrDefault() is { } widget)
                        _reorderHelper.RegisterRemove(widget);

                    break;
                }
            }
        }

        private async void ReorderHelper_Reordered(object? sender, BaseWidgetViewModel e)
        {
            await PersistWidgetOrderAsync();
        }

        private async Task PersistWidgetOrderAsync()
        {
            try
            {
                // Get the current order from the ObservableCollection
                var orderedWidgets = Widgets.Select(w => w.WidgetModel).ToArray();

                // Update the underlying collection model
                WidgetsCollectionModel.UpdateOrder(orderedWidgets);

                // Persist the changes
                await WidgetsCollectionModel.SaveAsync();
            }
            catch (Exception)
            {
            }
        }

        private BaseWidgetViewModel? GetWidgetForModel(IWidgetModel widgetModel)
        {
            return widgetModel.WidgetId switch
            {
                Constants.Widgets.HEALTH_WIDGET_ID => new HealthWidgetViewModel(_unlockedVaultViewModel, _dashboardNavigator, widgetModel),
                Constants.Widgets.GRAPHS_WIDGET_ID => new GraphsWidgetViewModel(_unlockedVaultViewModel, widgetModel),
                Constants.Widgets.AGGREGATED_DATA_WIDGET_ID => new AggregatedDataWidgetViewModel(_unlockedVaultViewModel, widgetModel),
                _ => null
            };
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            _reorderHelper.Reordered -= ReorderHelper_Reordered;
            _reorderHelper.Dispose();

            Widgets.CollectionChanged -= Widgets_CollectionChanged;
            Widgets.DisposeAll();
        }
    }
}
