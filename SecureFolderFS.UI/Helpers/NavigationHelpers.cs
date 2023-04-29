using SecureFolderFS.Sdk.Services;
using SecureFolderFS.UI.Controls;
using SecureFolderFS.UI.ServiceImplementation;

namespace SecureFolderFS.UI.Helpers
{
    public static class NavigationHelpers
    {
        public static void ResetNavigation<TNavigationControl>(this INavigationService navigationService)
            where TNavigationControl : class, INavigationControl
        {
            if (navigationService is INavigationControlContract<TNavigationControl> navigationControlContract)
                navigationControlContract.NavigationControl = null;
        }

        public static bool SetupNavigation<TNavigationControl>(this INavigationService navigationService, TNavigationControl navigationControl, bool overrideNavigation = false)
            where TNavigationControl : class, INavigationControl
        {
            if (!overrideNavigation && navigationService.IsInitialized)
                return true;

            if (navigationService is INavigationControlContract<TNavigationControl> navigationControlContract)
            {
                navigationControlContract.NavigationControl = navigationControl;
                return navigationService.IsInitialized;
            }

            return false;
        }
    }
}
