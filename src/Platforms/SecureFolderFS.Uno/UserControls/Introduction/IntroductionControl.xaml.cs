using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using SecureFolderFS.Sdk.ViewModels.Views.Overlays;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Shared.Extensions;
using SecureFolderFS.Shared.Models;
using SecureFolderFS.UI.Utils;
using SecureFolderFS.Uno.Extensions;
using SecureFolderFS.Uno.UserControls.InterfaceRoot;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace SecureFolderFS.Uno.UserControls.Introduction
{
    public sealed partial class IntroductionControl : UserControl, IOverlayControl
    {
        private Grid? _overlayContainer;

        public IntroductionOverlayViewModel? ViewModel
        {
            get => DataContext.TryCast<IntroductionOverlayViewModel>();
            set => DataContext = value;
        }

        public IntroductionControl()
        {
            InitializeComponent();
        }

        /// <inheritdoc/>
        public async Task<IResult> ShowAsync()
        {
            if (ViewModel is null)
                return Result.Failure(null); 
            
            if (ViewModel.TaskCompletion.Task.IsCompleted)
                return ViewModel.TaskCompletion.Task.Result;
            
            // Get the main window and its content
            var mainWindow = App.Instance?.MainWindow;
            if (mainWindow?.Content is not MainWindowRootControl rootControl)
                return Result.Failure(null);
            
            // Get the OverlayContainer from MainWindowRootControl
            _overlayContainer = rootControl.OverlayContainer;
            if (_overlayContainer is null)
                return Result.Failure(null);
            
            // Add this control to the overlay container
            _overlayContainer.Children.Add(this);
            
            // Set initial state for animation (invisible and slightly scaled down)
            RootGrid.Opacity = 0;
            
            // Show the overlay container
            _overlayContainer.Visibility = Visibility.Visible;
            
            // Play the show animation
            await ShowOverlayStoryboard.BeginAsync();
            ShowOverlayStoryboard.Stop();
            RootGrid.Opacity = 1;
            
            // Wait for the overlay to be closed
            return await ViewModel.TaskCompletion.Task;
        }

        /// <inheritdoc/>
        public void SetView(IViewable viewable) => ViewModel = (IntroductionOverlayViewModel)viewable;

        /// <inheritdoc/>
        public async Task HideAsync()
        {
            // Play the hide animation
            await HideOverlayStoryboard.BeginAsync();
            HideOverlayStoryboard.Stop();
            
            // Hide and clean up the overlay container
            if (_overlayContainer is not null)
            {
                _overlayContainer.Children.Remove(this);
                _overlayContainer.Visibility = Visibility.Collapsed;
                _overlayContainer = null;
            }
            
            ViewModel?.TaskCompletion.SetResult(ContentDialogResult.None.ParseOverlayOption());
        }
    }
}
