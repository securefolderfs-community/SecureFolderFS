using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using SecureFolderFS.Sdk.Enums;
using SecureFolderFS.Sdk.Extensions;
using SecureFolderFS.Sdk.ViewModels.Dialogs;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.UI.Utils;
using Windows.Media.Core;

// To learn more about WinUI, the WinUI project structure,D
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace SecureFolderFS.Uno.Dialogs
{
    public sealed partial class ExplanationDialog : ContentDialog, IOverlayControl
    {
        private IDisposable? _disposable;

        /// <inheritdoc/>
        public ExplanationDialogViewModel ViewModel
        {
            get => (ExplanationDialogViewModel)DataContext;
            set => DataContext = value;
        }

        public ExplanationDialog()
        {
            InitializeComponent();
        }

        /// <inheritdoc/>
        public new async Task<IResult> ShowAsync() => DialogExtensions.ResultFromDialogOption((DialogOption)await base.ShowAsync());

        /// <inheritdoc/>
        public void SetView(IView view) => ViewModel = (ExplanationDialogViewModel)view;

        private void Media_Loaded(object sender, RoutedEventArgs e)
        {
#if WINDOWS
            var assembly = AppDomain.CurrentDomain.GetAssemblies().SingleOrDefault(x => x.GetName().Name == "SecureFolderFS.UI")!;
            var stream = assembly.GetManifestResourceStream("SecureFolderFS.UI.Assets.AppAssets.Media.ExplanationVideoDark.mov");
            _disposable = stream;

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
            _disposable?.Dispose();
            Media.MediaPlayer?.Dispose();
#endif
        }
    }
}
