using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using SecureFolderFS.Sdk.Enums;
using SecureFolderFS.Sdk.Extensions;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.ViewModels.Dialogs;
using SecureFolderFS.Shared.Utils;
using System;
using System.Threading.Tasks;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace SecureFolderFS.WinUI.Dialogs
{
    public sealed partial class LicensesDialog : ContentDialog, IDialog<LicensesDialogViewModel>
    {
        /// <inheritdoc/>
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
        public new async Task<IResult> ShowAsync() => DialogExtensions.ResultFromDialogOption((DialogOption)await base.ShowAsync());

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
