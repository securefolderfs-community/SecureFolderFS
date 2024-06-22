using System.Threading.Tasks;
using Microsoft.UI.Xaml.Controls;
using SecureFolderFS.Sdk.Enums;
using SecureFolderFS.Sdk.Extensions;
using SecureFolderFS.Sdk.ViewModels.Views.Overlays;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Shared.Extensions;
using SecureFolderFS.Shared.Helpers;
using SecureFolderFS.UI.Utils;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace SecureFolderFS.Uno.UserControls.Introduction
{
    public sealed partial class IntroductionControl : UserControl, IOverlayControl
    {
        public IntroductionDialogViewModel? ViewModel
        {
            get => DataContext.TryCast<IntroductionDialogViewModel>();
            set => DataContext = value;
        }

        public IntroductionControl()
        {
            InitializeComponent();
        }

        /// <inheritdoc/>
        public Task<IResult> ShowAsync() => ViewModel?.TaskCompletion.Task ?? Task.FromResult<IResult>(Result.Failure(null));

        /// <inheritdoc/>
        public void SetView(IViewable viewable) => ViewModel = (IntroductionDialogViewModel)viewable;

        /// <inheritdoc/>
        public Task HideAsync()
        {
            ViewModel?.TaskCompletion.SetResult(DialogOption.Cancel.ParseOverlayOption());
            return Task.CompletedTask;
        }
    }
}
