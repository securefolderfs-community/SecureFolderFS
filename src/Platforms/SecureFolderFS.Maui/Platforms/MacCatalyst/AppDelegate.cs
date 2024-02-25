using System.Runtime.CompilerServices;
using Foundation;

namespace SecureFolderFS.Maui
{
    [Register("AppDelegate")]
    public sealed class AppDelegate : MauiUIApplicationDelegate
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
    }
}
