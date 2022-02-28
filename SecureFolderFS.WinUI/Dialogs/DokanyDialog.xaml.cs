using System;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Controls;
using SecureFolderFS.Backend.Dialogs;
using SecureFolderFS.Backend.Enums;
using SecureFolderFS.Backend.ViewModels.Dialogs;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

#nullable enable

namespace SecureFolderFS.WinUI.Dialogs
{
    public sealed partial class DokanyDialog : ContentDialog, IDialog<DokanyDialogViewModel>
    {
        public DokanyDialogViewModel ViewModel
        {
            get => (DokanyDialogViewModel)DataContext;
            set => DataContext = value;
        }

        public DokanyDialog()
        {
            this.InitializeComponent();
        }

        public new async Task<DialogResult> ShowAsync() => (DialogResult)await base.ShowAsync();

        private void ContentDialog_Closing(ContentDialog sender, ContentDialogClosingEventArgs args)
        {
            args.Cancel = true;
        }
    }
}
