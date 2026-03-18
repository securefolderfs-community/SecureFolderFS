using System;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using SecureFolderFS.Sdk.ViewModels.Controls.Components;
using SecureFolderFS.Sdk.ViewModels.Views.Overlays;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Shared.Extensions;
using SecureFolderFS.Shared.Helpers;
using SecureFolderFS.UI.Utils;
using SecureFolderFS.Uno.Extensions;
using WinUI.TableView;

// To learn more about WinUI, the WinUI project structure,D
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace SecureFolderFS.Uno.Dialogs
{
    public sealed partial class RecycleBinDialog : ContentDialog, IOverlayControl
    {
        private readonly FirstTimeHelper _firstTime;
        private bool _suppressToggle;
        private PickerOptionViewModel? _previousOption;

        public RecycleBinOverlayViewModel? ViewModel
        {
            get => DataContext.TryCast<RecycleBinOverlayViewModel>();
            set => DataContext = value;
        }

        public RecycleBinDialog()
        {
            _firstTime = new(2);
            InitializeComponent();
        }

        /// <inheritdoc/>
        public new async Task<IResult> ShowAsync() => (await base.ShowAsync()).ParseOverlayOption();

        /// <inheritdoc/>
        public void SetView(IViewable viewable) => ViewModel = (RecycleBinOverlayViewModel)viewable;

        /// <inheritdoc/>
        public Task HideAsync()
        {
            Hide();
            return Task.CompletedTask;
        }

        private async void ToggleSwitch_Toggled(object sender, RoutedEventArgs e)
        {
            if (ViewModel is null)
                return;

            if (_suppressToggle)
            {
                _suppressToggle = false;
                return;
            }

            if (_firstTime.IsFirstTime() && ViewModel.IsRecycleBinEnabled)
            {
                RecycleBinActionBlock.IsExpanded = ViewModel.IsRecycleBinEnabled;
                return;
            }

            if (sender is not ToggleSwitch toggleSwitch)
                return;

            await ViewModel.ToggleRecycleBinAsync(toggleSwitch.IsOn);
            RecycleBinActionBlock.IsExpanded = ViewModel.IsRecycleBinEnabled;

            if (ViewModel.IsRecycleBinEnabled != toggleSwitch.IsOn)
            {
                _suppressToggle = true;
                toggleSwitch.IsOn = ViewModel.IsRecycleBinEnabled;
            }
        }

        private async void SizeOptions_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ViewModel is null)
                return;

            _previousOption ??= ViewModel.CurrentSizeOption;
            if (_firstTime.IsFirstTime())
                return;

            await ViewModel.ToggleRecycleBinAsync(ViewModel.IsRecycleBinEnabled);
            await ViewModel.UpdateSizesAsync(_previousOption is null || _previousOption.Id == "-1");
            _previousOption = ViewModel.CurrentSizeOption;
        }

        private void RecycleBinTableView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ViewModel is null || sender is not TableView listView)
                return;

            ViewModel.IsSelecting = listView.SelectedItems.Count > 0;
        }

        private void RecycleBinTableView_BeginningEdit(object? sender, TableViewBeginningEditEventArgs e)
        {
            e.Cancel = true;
        }
    }
}
