using System;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Controls;
using SecureFolderFS.Sdk.Enums;
using SecureFolderFS.Sdk.Extensions;
using SecureFolderFS.Sdk.ViewModels.Dialogs;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.UI.Utils;

// To learn more about WinUI, the WinUI project structure,D
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace SecureFolderFS.Uno.Dialogs
{
    public sealed partial class PaymentDialog : ContentDialog, IOverlayControl
    {
        /// <inheritdoc/>
        public PaymentDialogViewModel ViewModel
        {
            get => (PaymentDialogViewModel)DataContext;
            set => DataContext = value;
        }

        public PaymentDialog()
        {
            InitializeComponent();
        }

        /// <inheritdoc/>
        public new async Task<IResult> ShowAsync() => DialogExtensions.ResultFromDialogOption((DialogOption)await base.ShowAsync());

        /// <inheritdoc/>
        public void SetView(IView view) => ViewModel = (PaymentDialogViewModel)view;
    }
}
