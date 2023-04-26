using SecureFolderFS.Sdk.Services;
using SecureFolderFS.UI.Controls;
using SecureFolderFS.UI.ServiceImplementation;

namespace SecureFolderFS.UI.Helpers
{
    public static class NavigationHelpers
    {
        public static bool SetupNavigation<TNavigationControl>(this INavigationService navigationService, TNavigationControl navigationControl, bool overrideNavigation = false)
            where TNavigationControl : class, INavigationControl
        {
            if (!overrideNavigation && navigationService.IsInitialized)
                return true;

            if (navigationService is INavigationContract<TNavigationControl> navigationServiceContract)
            {
                navigationServiceContract.NavigationControl = navigationControl;
                return navigationServiceContract.IsInitialized;
            }

            return false;
        }
    }
}
