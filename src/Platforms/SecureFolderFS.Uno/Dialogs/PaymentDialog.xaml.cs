using System;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Controls;
using SecureFolderFS.Sdk.Enums;
using SecureFolderFS.Sdk.Extensions;
using SecureFolderFS.Sdk.ViewModels.Views.Overlays;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.UI.Utils;

// To learn more about WinUI, the WinUI project structure,D
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace SecureFolderFS.Uno.Dialogs
{
    public sealed partial class PaymentDialog : ContentDialog, IOverlayControl
    {
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
        public new async Task<IResult> ShowAsync() => ((DialogOption)await base.ShowAsync()).ParseDialogOption();

        /// <inheritdoc/>
        public void SetView(IViewable viewable) => ViewModel = (PaymentDialogViewModel)viewable;

        /// <inheritdoc/>
        public Task HideAsync()
        {
            ViewModel?.OnDisappearing();
            Hide();
            return Task.CompletedTask;
        }
    }
}
