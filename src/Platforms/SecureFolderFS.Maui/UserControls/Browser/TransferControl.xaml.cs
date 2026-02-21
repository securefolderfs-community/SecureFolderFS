using System.Windows.Input;

namespace SecureFolderFS.Maui.UserControls.Browser
{
    public partial class TransferControl : ContentView
    {
        private const double BOUNCE_LIMIT = -16d;
        private const double HIDE_TRANSLATION = 200d;
        private const double DISMISS_THRESHOLD = 40d;
        private bool _isDismissing;

        public TransferControl()
        {
            InitializeComponent();
        }

        private async void OnPanUpdated(object? sender, PanUpdatedEventArgs e)
        {
            if (_isDismissing)
                return;

            switch (e.StatusType)
            {
                case GestureStatus.Running:
                {
                    var translation = e.TotalY;
                    RootPanel.TranslationY = translation < 0
                        ? Math.Max(translation / 4d, BOUNCE_LIMIT)
                        : translation;
                    break;
                }

                case GestureStatus.Completed:
                case GestureStatus.Canceled:
                {
                    if (RootPanel.TranslationY >= DISMISS_THRESHOLD)
                    {
                        _isDismissing = true;

                        // Animate out from the current dragged position
                        var currentY = RootPanel.TranslationY;
                        var remainingDistance = HIDE_TRANSLATION - currentY;
                        var duration = (uint)Math.Max(300d * (remainingDistance / HIDE_TRANSLATION), 150d);
                        await RootPanel.TranslateToAsync(0, HIDE_TRANSLATION, duration, Easing.CubicInOut);

                        // Clean up visual state
                        RootPanel.IsVisible = false;
                        RootPanel.TranslationY = 0d;

                        _isDismissing = false;

                        // Tell the caller - they will set IsShown=false, which the guard will skip animating.
                        // This also ensures the backing value is actually false, so the next IsShown=true fires propertyChanged.
                        CancelCommand?.Execute(null);
                    }
                    else
                    {
                        await RootPanel.TranslateToAsync(0, 0, 250U, Easing.SpringOut);
                    }
                    break;
                }
            }
        }

        public bool IsShown
        {
            get => (bool)GetValue(IsShownProperty);
            set => SetValue(IsShownProperty, value);
        }
        public static readonly BindableProperty IsShownProperty =
            BindableProperty.Create(nameof(IsShown), typeof(bool), typeof(TransferControl), false,
                propertyChanged: static async (bindable, _, newValue) =>
                {
                    if (newValue is not bool bValue || bindable is not TransferControl tc)
                        return;

                    if (tc._isDismissing)
                    {
                        // Gesture already handled the animation; just ensure a clean state
                        tc.RootPanel.IsVisible = false;
                        tc.RootPanel.TranslationY = 0d;
                        return;
                    }

                    if (bValue)
                    {
                        tc.RootPanel.TranslationY = HIDE_TRANSLATION;
                        tc.RootPanel.IsVisible = true;
                        await tc.RootPanel.TranslateToAsync(0, 0, 350U, Easing.CubicInOut);
                    }
                    else
                    {
                        var currentY = tc.RootPanel.TranslationY;
                        var remainingDistance = HIDE_TRANSLATION - currentY;
                        var duration = (uint)Math.Max(350d * (remainingDistance / HIDE_TRANSLATION), 150d);
                        await tc.RootPanel.TranslateToAsync(0, HIDE_TRANSLATION, duration, Easing.CubicInOut);
                        tc.RootPanel.IsVisible = false;
                        tc.RootPanel.TranslationY = 0d;
                    }
                });

        public bool CanCancel
        {
            get => (bool)GetValue(CanCancelProperty);
            set => SetValue(CanCancelProperty, value);
        }
        public static readonly BindableProperty CanCancelProperty =
            BindableProperty.Create(nameof(CanCancel), typeof(bool), typeof(TransferControl), true);

        public bool IsProgressing
        {
            get => (bool)GetValue(IsProgressingProperty);
            set => SetValue(IsProgressingProperty, value);
        }
        public static readonly BindableProperty IsProgressingProperty =
            BindableProperty.Create(nameof(IsProgressing), typeof(bool), typeof(TransferControl), false);

        public string? Title
        {
            get => (string?)GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }
        public static readonly BindableProperty TitleProperty =
            BindableProperty.Create(nameof(Title), typeof(string), typeof(TransferControl));

        public string? PrimaryButtonText
        {
            get => (string?)GetValue(PrimaryButtonTextProperty);
            set => SetValue(PrimaryButtonTextProperty, value);
        }
        public static readonly BindableProperty PrimaryButtonTextProperty =
            BindableProperty.Create(nameof(PrimaryButtonText), typeof(string), typeof(TransferControl));

        public ICommand? CancelCommand
        {
            get => (ICommand?)GetValue(CancelCommandProperty);
            set => SetValue(CancelCommandProperty, value);
        }
        public static readonly BindableProperty CancelCommandProperty =
            BindableProperty.Create(nameof(CancelCommand), typeof(ICommand), typeof(TransferControl));

        public ICommand? PrimaryCommand
        {
            get => (ICommand?)GetValue(PrimaryCommandProperty);
            set => SetValue(PrimaryCommandProperty, value);
        }
        public static readonly BindableProperty PrimaryCommandProperty =
            BindableProperty.Create(nameof(PrimaryCommand), typeof(ICommand), typeof(TransferControl));
    }
}
