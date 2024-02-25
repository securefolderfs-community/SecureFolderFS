using SecureFolderFS.Sdk.Services;
using System;
using System.Linq;
using System.Threading.Tasks;
using SecureFolderFS.Shared.ComponentModel;

namespace SecureFolderFS.Sdk.Extensions
{
    public static class NavigationExtensions
    {
        public static T? TryGetView<T>(this INavigationService navigationService)
            where T : class, IViewDesignation
        {
            return (T?)navigationService.Views.FirstOrDefault(x => x is T);
        }

        public static async Task<bool> TryNavigateAsync<T>(this INavigationService navigationService, Func<T>? initializer = null, bool useInitialization = true)
            where T : class, IViewDesignation
        {
            var view = navigationService.TryGetView<T>();
            var isNewView = view is null;

            view ??= initializer?.Invoke() ?? null;
            if (view is null)
                return false;

            // Initialize if the target supports IAsyncInitialize and doesn't already exist
            if (isNewView && useInitialization && view is IAsyncInitialize supportsAsyncInitialize)
                _ = supportsAsyncInitialize.InitAsync();

            // Navigate to the target
            return await navigationService.NavigateAsync(view);
        }

        public static async Task<bool> TryNavigateAndForgetAsync(this INavigationService navigationService, IViewDesignation view)
        {
            var navigated = false;
            IViewDesignation? currentView = null;

            try
            {
                if (navigationService.CurrentView is not null)
                {
                    navigationService.Views.Remove(navigationService.CurrentView);
                    currentView = navigationService.CurrentView;
                }

                navigated = await navigationService.NavigateAsync(view);
                return navigated;
            }
            finally
            {
                if (navigated)
                    (currentView as IDisposable)?.Dispose();
            }
        }
    }
}
