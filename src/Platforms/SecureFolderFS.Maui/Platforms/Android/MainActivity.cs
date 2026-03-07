using System.ComponentModel;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Views;
using AndroidX.Activity;
using AndroidX.Core.View;
using Microsoft.Maui.Platform;
using SecureFolderFS.Maui.Helpers;
using SecureFolderFS.UI.Enums;

namespace SecureFolderFS.Maui
{
    [Activity(
        Theme = "@style/Maui.SplashTheme",
        Exported = true,
        MainLauncher = true,
        WindowSoftInputMode = SoftInput.AdjustResize,
        ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density,
        ResizeableActivity = true,
        LaunchMode = LaunchMode.SingleTask)]
    public class MainActivity : MauiAppCompatActivity
    {
        public static MainActivity? Instance { get; private set; }

        public Action<int, Result, Intent?>? ActivityResult { get; set; }

        public MainActivity()
        {
            Instance ??= this;
        }

        /// <inheritdoc/>
        protected override void OnCreate(Bundle? savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Enable edge to edge
            EdgeToEdge.Enable(this);

            // Apply system bar insets to the root view so the Shell toolbar
            // is not hidden behind the status bar on navigated pages.
            var rootView = FindViewById(Android.Resource.Id.Content);
            if (rootView is not null)
            {
                ViewCompat.SetOnApplyWindowInsetsListener(rootView, new WindowInsetsListener());
            }

            // Make the navigation bar transparent so content draws behind it
#pragma warning disable CA1422
            Window?.SetNavigationBarColor(Android.Graphics.Color.Transparent);
#pragma warning restore CA1422

            // Configure StatusBar color
            ApplyStatusBarColor(MauiThemeHelper.Instance.CurrentTheme);

            // Always listen for theme changes
            MauiThemeHelper.Instance.PropertyChanged += ThemeHelper_PropertyChanged;
        }

        /// <inheritdoc/>
        protected override void OnActivityResult(int requestCode, Result resultCode, Intent? data)
        {
            base.OnActivityResult(requestCode, resultCode, data);
            ActivityResult?.Invoke(requestCode, resultCode, data);
        }

        private void ThemeHelper_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName != nameof(MauiThemeHelper.CurrentTheme))
                return;

            ApplyStatusBarColor(MauiThemeHelper.Instance.CurrentTheme);
        }

        private void ApplyStatusBarColor(ThemeType themeType)
        {
#pragma warning disable CA1422
            Window?.SetStatusBarColor((App.Instance.Resources[themeType switch
            {
                ThemeType.Dark => "PrimaryDarkColor",
                _ => "PrimaryLightColor"
            }] as Color)!.ToPlatform());
#pragma warning restore CA1422
        }

        private sealed class WindowInsetsListener : Java.Lang.Object, IOnApplyWindowInsetsListener
        {
            public WindowInsetsCompat? OnApplyWindowInsets(Android.Views.View? view, WindowInsetsCompat? insets)
            {
                if (view is null || insets is null)
                    return insets;

                // Only apply top (status bar) padding so the toolbar is visible.
                // Bottom padding is NOT applied so content extends behind the
                // navigation bar for a modern edge-to-edge experience.
                var statusBarInsets = insets.GetInsets(WindowInsetsCompat.Type.StatusBars());
                var imeInsets = insets.GetInsets(WindowInsetsCompat.Type.Ime());

                // When the soft keyboard (IME) is visible, use its bottom inset
                // so that input fields are not hidden behind the keyboard.
                var bottomPadding = imeInsets is not null && imeInsets.Bottom > 0 ? imeInsets.Bottom : 0;

                view.SetPadding(
                    statusBarInsets?.Left ?? 0,
                    statusBarInsets?.Top ?? 0,
                    statusBarInsets?.Right ?? 0,
                    bottomPadding);

                return WindowInsetsCompat.Consumed;
            }
        }
    }
}
