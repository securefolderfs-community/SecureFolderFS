using System.ComponentModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using SecureFolderFS.Shared.ComponentModel;

namespace SecureFolderFS.Sdk.ViewModels.Controls.Widgets
{
    [Bindable(true)]
    public sealed partial class AvailableWidgetViewModel : ObservableObject, IViewable
    {
        [ObservableProperty] private string? _Title;
        [ObservableProperty] private ICommand? _AddWidgetCommand;

        /// <summary>
        /// Gets the unique identifier of the widget.
        /// </summary>
        public string WidgetId { get; }

        public AvailableWidgetViewModel(string widgetId, ICommand? addWidgetCommand)
        {
            WidgetId = widgetId;
            AddWidgetCommand = addWidgetCommand;
        }
    }
}
