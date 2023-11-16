using Microsoft.UI.Xaml.Controls;
using SecureFolderFS.Sdk.Enums;
using SecureFolderFS.Sdk.Extensions;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.ViewModels.Dialogs;
using SecureFolderFS.Shared.Utilities;
using System;
using System.Threading.Tasks;

// To learn more about WinUI, the WinUI project structure,D
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace SecureFolderFS.WinUI.Dialogs
{
    public sealed partial class AgreementDialog : ContentDialog, IDialog<AgreementDialogViewModel>
    {
        private bool _agreed;

        /// <inheritdoc/>
        public AgreementDialogViewModel ViewModel
        {
            get => (AgreementDialogViewModel)DataContext;
            set => DataContext = value;
        }

        public AgreementDialog()
        {
            InitializeComponent();
        }

        /// <inheritdoc/>
        public new async Task<IResult> ShowAsync() => DialogExtensions.ResultFromDialogOption((DialogOption)await base.ShowAsync());

        private void AgreementDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            _agreed = true;
        }

        private void AgreementDialog_Closing(ContentDialog sender, ContentDialogClosingEventArgs args)
        {
            args.Cancel = !_agreed;
        }
    }
}
