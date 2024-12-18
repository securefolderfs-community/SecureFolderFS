using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.UI.ServiceImplementation;
using SecureFolderFS.UI.Utils;

namespace SecureFolderFS.UI.Helpers
{
    public static class NavigationHelpers
    {
        public static void ResetNavigation(this INavigationService navigationService)
        {
            if (navigationService is INavigationControlContract navigationControlContract)
                navigationControlContract.Navigator = null;
        }

        public static bool SetupNavigation(this INavigationService navigationService, INavigator navigator, bool overrideNavigation = false)
        {
            if (navigationService is not BaseNavigationService baseNavigationService)
                return false;

            if (!overrideNavigation && baseNavigationService.IsInitialized)
                return true;

            baseNavigationService.Navigator = navigator;
            return baseNavigationService.IsInitialized;
        }
    }
}
