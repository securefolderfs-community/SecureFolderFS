using System.ComponentModel;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using AndroidX.Activity;
using Microsoft.Maui.Platform;
using SecureFolderFS.Maui.Helpers;
using SecureFolderFS.UI.Enums;

namespace SecureFolderFS.Maui
{
    [Activity(
        Theme = "@style/Maui.SplashTheme",
        Exported = true,
        MainLauncher = true,
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
    }
}
