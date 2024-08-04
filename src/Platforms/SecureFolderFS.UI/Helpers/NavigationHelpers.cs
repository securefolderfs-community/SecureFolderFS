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
            if (navigationService is not BaseNavigationService baseNavigationService)
                return false;

            if (!overrideNavigation && baseNavigationService.IsInitialized)
                return true;

            baseNavigationService.NavigationControl = navigationControl;
            return baseNavigationService.IsInitialized;
        }
    }
}
