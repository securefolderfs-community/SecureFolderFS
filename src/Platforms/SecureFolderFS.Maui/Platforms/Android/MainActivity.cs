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

                var systemBarsInsets = insets.GetInsets(WindowInsetsCompat.Type.SystemBars());
                if (systemBarsInsets is not null)
                    view.SetPadding(systemBarsInsets.Left, systemBarsInsets.Top, systemBarsInsets.Right, systemBarsInsets.Bottom);
                
                return WindowInsetsCompat.Consumed;
            }
        }
    }
}
