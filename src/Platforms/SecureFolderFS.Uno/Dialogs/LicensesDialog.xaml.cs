using System;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using SecureFolderFS.Sdk.Enums;
using SecureFolderFS.Sdk.Extensions;
using SecureFolderFS.Sdk.ViewModels.Views.Overlays;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.UI.Utils;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace SecureFolderFS.Uno.Dialogs
{
    public sealed partial class LicensesDialog : ContentDialog, IOverlayControl
    {
        public LicensesDialogViewModel ViewModel
        {
            get => (LicensesDialogViewModel)DataContext;
            set => DataContext = value;
        }

        public LicensesDialog()
        {
            InitializeComponent();
        }

        /// <inheritdoc/>
        public new async Task<IResult> ShowAsync() => ((DialogOption)await base.ShowAsync()).ParseDialogOption();

        /// <inheritdoc/>
        public void SetView(IViewable viewable) => ViewModel = (LicensesDialogViewModel)viewable;

        /// <inheritdoc/>
        public Task HideAsync()
        {
            Hide();
            return Task.CompletedTask;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Hide();
        }

        private void Licenses_Loaded(object sender, RoutedEventArgs e)
        {
            _ = ViewModel.InitAsync();
        }
    }
}
