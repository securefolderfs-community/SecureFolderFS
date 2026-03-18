using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SecureFolderFS.Sdk.AppModels;
using SecureFolderFS.Sdk.DataModels;
using SecureFolderFS.Sdk.Extensions;
using SecureFolderFS.Sdk.Helpers;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.ViewModels.Controls.Widgets.Data;
using SecureFolderFS.Sdk.ViewModels.Controls.Widgets.Health;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Shared.Extensions;

namespace SecureFolderFS.Sdk.ViewModels.Controls.Widgets
{
    [Bindable(true)]
    public sealed partial class WidgetsListViewModel : ObservableObject, IAsyncInitialize, IDisposable
    {
        private readonly INavigator _dashboardNavigator;
        private readonly UnlockedVaultViewModel _unlockedVaultViewModel;
        private readonly SynchronizationContext? _synchronizationContext;
        private readonly CollectionReorderHelper<BaseWidgetViewModel> _reorderHelper;
        private bool _suppressCollectionChanged;

        public IWidgetsCollectionModel WidgetsCollectionModel { get; }

        public ObservableCollection<BaseWidgetViewModel> Widgets { get; }

        public ObservableCollection<AvailableWidgetViewModel> AvailableWidgets { get; }

        public WidgetsListViewModel(UnlockedVaultViewModel unlockedVaultViewModel, INavigator dashboardNavigator, IWidgetsCollectionModel widgetsCollectionModel)
        {
            _dashboardNavigator = dashboardNavigator;
            _unlockedVaultViewModel = unlockedVaultViewModel;
            _synchronizationContext = SynchronizationContext.Current;
            _reorderHelper = new();
            WidgetsCollectionModel = widgetsCollectionModel;
            AvailableWidgets = new();
            Widgets = new();

            // Subscribe to collection changes to persist reordering
            Widgets.CollectionChanged += Widgets_CollectionChanged;
            _reorderHelper.Reordered += ReorderHelper_Reordered;
        }

        /// <inheritdoc/>
        public async Task InitAsync(CancellationToken cancellationToken = default)
        {
            await _synchronizationContext.PostOrExecuteAsync(async () =>
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

                var widgetIds = Widgets.Select(x => x.WidgetModel.WidgetId).ToArray();
                if (!widgetIds.Contains(Constants.Widgets.HEALTH_WIDGET_ID))
                    AvailableWidgets.Add(new(Constants.Widgets.HEALTH_WIDGET_ID, AddWidgetCommand) { Title = "HealthWidget".ToLocalized() });

                if (!widgetIds.Contains(Constants.Widgets.GRAPHS_WIDGET_ID))
                    AvailableWidgets.Add(new(Constants.Widgets.GRAPHS_WIDGET_ID, AddWidgetCommand) { Title = "GraphsWidget".ToLocalized() });

                if (!widgetIds.Contains(Constants.Widgets.AGGREGATED_DATA_WIDGET_ID))
                    AvailableWidgets.Add(new(Constants.Widgets.AGGREGATED_DATA_WIDGET_ID, AddWidgetCommand) { Title = "AggregatedDataWidget".ToLocalized() });
            });
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
                Constants.Widgets.HEALTH_WIDGET_ID => new HealthWidgetViewModel(_unlockedVaultViewModel, _dashboardNavigator, widgetModel) { RemoveWidgetCommand = RemoveWidgetCommand },
                Constants.Widgets.GRAPHS_WIDGET_ID => new GraphsWidgetViewModel(_unlockedVaultViewModel, widgetModel) { RemoveWidgetCommand = RemoveWidgetCommand },
                Constants.Widgets.AGGREGATED_DATA_WIDGET_ID => new AggregatedDataWidgetViewModel(_unlockedVaultViewModel, widgetModel) { RemoveWidgetCommand = RemoveWidgetCommand },
                _ => null
            };
        }

        [RelayCommand]
        private async Task AddWidgetAsync(AvailableWidgetViewModel? widget, CancellationToken cancellationToken)
        {
            if (widget is null)
                return;

            var dataModel = new WidgetDataModel(widget.WidgetId);
            var widgetModel = new WidgetModel(widget.WidgetId, WidgetsCollectionModel.VaultWidgets, dataModel);
            var widgetViewModel = GetWidgetForModel(widgetModel);
            if (widgetViewModel is null)
                return;

            _suppressCollectionChanged = true;
            Widgets.Add(widgetViewModel.WithInitAsync());
            AvailableWidgets.Remove(widget);

            await PersistWidgetOrderAsync();
        }

        [RelayCommand]
        private async Task RemoveWidgetAsync(BaseWidgetViewModel? widget, CancellationToken cancellationToken)
        {
            if (widget is null)
                return;

            _suppressCollectionChanged = true;
            if (!Widgets.Remove(widget))
                return;

            widget.Dispose();
            await PersistWidgetOrderAsync();

            var availableWidget = new AvailableWidgetViewModel(widget.WidgetModel.WidgetId, AddWidgetCommand) { Title = widget.Title };
            var index = widget.WidgetModel.WidgetId switch
            {
                Constants.Widgets.HEALTH_WIDGET_ID => 0,
                Constants.Widgets.GRAPHS_WIDGET_ID => 1,
                Constants.Widgets.AGGREGATED_DATA_WIDGET_ID => 2,
                _ => 0
            };

            AvailableWidgets.Insert(Math.Max(0, Math.Min(index, AvailableWidgets.Count - 1)), availableWidget);
        }

        private void Widgets_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (_suppressCollectionChanged)
            {
                _suppressCollectionChanged = false;
                return;
            }

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
