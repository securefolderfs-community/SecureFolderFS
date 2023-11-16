using Microsoft.UI.Xaml.Controls;
using SecureFolderFS.Sdk.Enums;
using SecureFolderFS.Sdk.Extensions;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.ViewModels.Dialogs;
using SecureFolderFS.Shared.Utilities;
using SecureFolderFS.UI.Utils;
using System.Threading.Tasks;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace SecureFolderFS.WinUI.UserControls.Introduction
{
    public sealed partial class IntroductionControl : UserControl, IDialog<IntroductionDialogViewModel>, IOverlayable
    {
        /// <inheritdoc/>
        public IntroductionDialogViewModel ViewModel
        {
            get => (IntroductionDialogViewModel)DataContext;
            set => DataContext = value;
        }

        /// <inheritdoc/>
        public object OverlayContent => this;

        public IntroductionControl()
        {
            InitializeComponent();
        }

        /// <inheritdoc/>
        public Task<IResult> ShowAsync()
        {
            return ViewModel.TaskCompletion.Task;
        }

        /// <inheritdoc/>
        public void Hide()
        {
            ViewModel.TaskCompletion.SetResult(DialogExtensions.ResultFromDialogOption(DialogOption.Cancel));
        }
    }
}
