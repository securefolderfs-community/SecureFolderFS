using System;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using SecureFolderFS.Sdk.ViewModels.Controls;
using SecureFolderFS.Sdk.ViewModels.Views.Overlays;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Shared.Extensions;
using SecureFolderFS.UI.Utils;
using SecureFolderFS.Uno.Extensions;

// To learn more about WinUI, the WinUI project structure,D
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace SecureFolderFS.Uno.Dialogs
{
    public sealed partial class RecycleBinDialog : ContentDialog, IOverlayControl
    {
        private PickerOptionViewModel? _previousOption;

        public RecycleBinOverlayViewModel? ViewModel
        {
            get => DataContext.TryCast<RecycleBinOverlayViewModel>();
            set => DataContext = value;
        }

        public RecycleBinDialog()
        {
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

        private async void RecycleBinSwitch_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (ViewModel is null)
                return;

            await ViewModel.ToggleRecycleBinAsync(ViewModel.IsRecycleBinEnabled);
            await ViewModel.UpdateSizesAsync(_previousOption is null || _previousOption.Id == "-1");
            _previousOption = ViewModel.CurrentSizeOption;
        }
    }
}
