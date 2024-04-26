using SecureFolderFS.Sdk.Services;
using SecureFolderFS.UI.ServiceImplementation;
using SecureFolderFS.UI.Utils;

namespace SecureFolderFS.UI.Helpers
{
    public static class NavigationHelpers
    {
        public static void ResetNavigation(this INavigationService navigationService)
        {
            if (navigationService is INavigationControlContract navigationControlContract)
                navigationControlContract.NavigationControl = null;
        }

        public static bool SetupNavigation(this INavigationService navigationService, INavigationControl navigationControl, bool overrideNavigation = false)
        {
            if (!overrideNavigation && navigationService.IsInitialized)
                return true;

            if (navigationService is INavigationControlContract navigationControlContract)
            {
                navigationControlContract.NavigationControl = navigationControl;
                return navigationService.IsInitialized;
            }

            return false;
        }
    }
}
