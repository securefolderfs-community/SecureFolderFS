using Android.App;
using Android.Content;
using Android.Content.PM;

namespace SecureFolderFS.Maui
{
    [Activity(Theme = "@style/Maui.SplashTheme", Exported = true, MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
    public class MainActivity : MauiAppCompatActivity
    {
        public static MainActivity? Instance { get; private set; }

        public Action<int, Result, Intent?>? ActivityResult { get; set; }

        public MainActivity()
        {
            Instance ??= this;
        }

        /// <inheritdoc/>
        protected override void OnActivityResult(int requestCode, Result resultCode, Intent? data)
        {
            base.OnActivityResult(requestCode, resultCode, data);
            ActivityResult?.Invoke(requestCode, resultCode, data);
        }
    }
}
