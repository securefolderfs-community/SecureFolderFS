using System;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using SecureFolderFS.Sdk.ViewModels.Views.Overlays;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Shared.Extensions;
using SecureFolderFS.UI.Utils;
using SecureFolderFS.Uno.Extensions;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace SecureFolderFS.Uno.Dialogs
{
    public sealed partial class PaymentDialog : ContentDialog, IOverlayControl
    {
        public PaymentOverlayViewModel? ViewModel
        {
            get => DataContext.TryCast<PaymentOverlayViewModel>();
            set => DataContext = value;
        }

        public PaymentDialog()
        {
            InitializeComponent();
        }

        /// <inheritdoc/>
        public new async Task<IResult> ShowAsync() => (await base.ShowAsync()).ParseOverlayOption();

        /// <inheritdoc/>
        public void SetView(IViewable viewable) => ViewModel = (PaymentOverlayViewModel)viewable;

        /// <inheritdoc/>
        public Task HideAsync()
        {
            Hide();
            return Task.CompletedTask;
        }

        private void PaymentDialog_Loaded(object sender, RoutedEventArgs e)
        {
            ((Storyboard)Resources["OuterRingBreathe"]).Begin();
            ((Storyboard)Resources["InnerRingBreathe"]).Begin();
        }
        
        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Hide();
        }
    }
}
