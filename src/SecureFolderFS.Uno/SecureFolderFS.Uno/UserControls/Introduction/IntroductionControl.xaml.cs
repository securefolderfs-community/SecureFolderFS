using System.Threading.Tasks;
using Microsoft.UI.Xaml.Controls;
using SecureFolderFS.Sdk.Enums;
using SecureFolderFS.Sdk.Extensions;
using SecureFolderFS.Sdk.ViewModels.Dialogs;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.UI.Utils;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace SecureFolderFS.Uno.UserControls.Introduction
{
    public sealed partial class IntroductionControl : UserControl, IOverlayControl
    {
        public IntroductionDialogViewModel ViewModel
        {
            get => (IntroductionDialogViewModel)DataContext;
            set => DataContext = value;
        }

        public IntroductionControl()
        {
            InitializeComponent();
        }

        /// <inheritdoc/>
        public Task<IResult> ShowAsync() => ViewModel.TaskCompletion.Task;

        /// <inheritdoc/>
        public void SetView(IView view) => ViewModel = (IntroductionDialogViewModel)view;

        /// <inheritdoc/>
        public void Hide()
        {
            ViewModel.TaskCompletion.SetResult(DialogExtensions.ResultFromDialogOption(DialogOption.Cancel));
        }
    }
}
