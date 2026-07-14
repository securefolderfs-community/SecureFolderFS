using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
#if __UNO_SKIA_X11__
using Microsoft.UI.Xaml.Input;
using SecureFolderFS.Uno.Platforms.Desktop.Helpers;
#endif

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace SecureFolderFS.Uno.UserControls
{
    public sealed partial class TitleBarControl : UserControl
    {
        private Window? _window;

        public TitleBarControl()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Shows client-drawn caption buttons and binds them to the specified <paramref name="window"/>.
        /// Used on platforms where the OS does not paint window buttons when the title bar is customized.
        /// </summary>
        /// <param name="window">The window that the caption buttons control.</param>
        public void ShowWindowButtons(Window window)
        {
            if (_window is not null)
                _window.AppWindow.Changed -= AppWindow_Changed;

            _window = window;
            _window.AppWindow.Changed += AppWindow_Changed;
            IsWindowButtonsVisible = true;
            UpdateMaximizeRestoreGlyph();

#if __UNO_SKIA_X11__
            // Uno's X11 backend does not implement SetTitleBar dragging, so initiate the move manually
            DragRegion.PointerPressed -= DragRegion_PointerPressed;
            DragRegion.PointerPressed += DragRegion_PointerPressed;
#endif
        }

#if __UNO_SKIA_X11__
        private void DragRegion_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            if (_window is null)
                return;

            if (!e.GetCurrentPoint(null).Properties.IsLeftButtonPressed)
                return;

            if (X11WindowHelper.TryBeginWindowDrag(_window))
                e.Handled = true;
        }
#endif

        private void UpdateMaximizeRestoreGlyph()
        {
            if (MaximizeRestoreIcon is null)
                return;

            var isMaximized = _window?.AppWindow.Presenter is OverlappedPresenter { State: OverlappedPresenterState.Maximized };
            MaximizeRestoreIcon.Glyph = isMaximized ? "\uE923" : "\uE922";
        }

        private void AppWindow_Changed(AppWindow sender, AppWindowChangedEventArgs args)
        {
            UpdateMaximizeRestoreGlyph();
        }

        private void WindowButtons_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateMaximizeRestoreGlyph();
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            if (_window?.AppWindow.Presenter is OverlappedPresenter presenter)
                presenter.Minimize();
        }

        private void MaximizeButton_Click(object sender, RoutedEventArgs e)
        {
            if (_window is not { } window || window.AppWindow.Presenter is not OverlappedPresenter presenter)
                return;

            if (presenter.State == OverlappedPresenterState.Maximized)
            {
                var restored = false;
#if __UNO_SKIA_X11__
                // Uno's X11 OverlappedPresenter.Restore only un-minimizes and never leaves
                // the maximized state, so clear the window manager's maximized state directly
                restored = X11WindowHelper.TryUnmaximizeWindow(window);
#endif
                if (!restored)
                    presenter.Restore();
            }
            else
            {
                presenter.Maximize();
            }

            UpdateMaximizeRestoreGlyph();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            _window?.Close();
        }

        public string? PrimaryTitle
        {
            get => (string?)GetValue(PrimaryTitleProperty);
            set => SetValue(PrimaryTitleProperty, value);
        }
        public static readonly DependencyProperty PrimaryTitleProperty =
            DependencyProperty.Register(nameof(PrimaryTitle), typeof(string), typeof(TitleBarControl), new PropertyMetadata(null));

        public string? SecondaryTitle
        {
            get => (string?)GetValue(SecondaryTitleProperty);
            set => SetValue(SecondaryTitleProperty, value);
        }
        public static readonly DependencyProperty SecondaryTitleProperty =
            DependencyProperty.Register(nameof(SecondaryTitle), typeof(string), typeof(TitleBarControl), new PropertyMetadata(null));

        public bool IsWindowButtonsVisible
        {
            get => (bool)GetValue(IsWindowButtonsVisibleProperty);
            set => SetValue(IsWindowButtonsVisibleProperty, value);
        }
        public static readonly DependencyProperty IsWindowButtonsVisibleProperty =
            DependencyProperty.Register(nameof(IsWindowButtonsVisible), typeof(bool), typeof(TitleBarControl), new PropertyMetadata(false));
    }
}
