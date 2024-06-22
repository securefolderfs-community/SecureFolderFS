using System;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using SecureFolderFS.Sdk.Enums;
using SecureFolderFS.Sdk.Extensions;
using SecureFolderFS.Sdk.ViewModels.Views.Overlays;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Shared.Extensions;
using SecureFolderFS.UI.Utils;


#if WINDOWS
using System.IO;
using System.Linq;
using Windows.Media.Core;
#endif

// To learn more about WinUI, the WinUI project structure,D
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace SecureFolderFS.Uno.Dialogs
{
    public sealed partial class ExplanationDialog : ContentDialog, IOverlayControl
    {
        private IDisposable? _streamDisposable;

        public ExplanationOverlayViewModel? ViewModel
        {
            get => DataContext.TryCast<ExplanationOverlayViewModel>();
            set => DataContext = value;
        }

        public ExplanationDialog()
        {
            InitializeComponent();
        }

        /// <inheritdoc/>
        public new async Task<IResult> ShowAsync() => ((DialogOption)await base.ShowAsync()).ParseOverlayOption();

        /// <inheritdoc/>
        public void SetView(IViewable viewable) => ViewModel = (ExplanationOverlayViewModel)viewable;

        /// <inheritdoc/>
        public Task HideAsync()
        {
            Hide();
            return Task.CompletedTask;
        }

        private void Media_Loaded(object sender, RoutedEventArgs e)
        {
#if WINDOWS
            var assembly = AppDomain.CurrentDomain.GetAssemblies().SingleOrDefault(x => x.GetName().Name == "SecureFolderFS.UI")!;
            var stream = assembly.GetManifestResourceStream("SecureFolderFS.UI.Assets.AppAssets.Media.ExplanationVideoDark.mov");
            _streamDisposable = stream;

            if (stream is null)
                return;

            Media.Source = MediaSource.CreateFromStream(stream.AsRandomAccessStream(), string.Empty);
            Media.MediaPlayer.IsLoopingEnabled = true;
            Media.MediaPlayer.AutoPlay = true;
            Media.MediaPlayer.Volume = 0.0d;
#endif
        }

        private void ExplanationDialog_Closing(ContentDialog sender, ContentDialogClosingEventArgs args)
        {
#if WINDOWS
            _streamDisposable?.Dispose();
            Media.MediaPlayer?.Dispose();
#endif
        }
    }
}
