using SecureFolderFS.Sdk.Services;
using System;
using System.Linq;
using System.Threading.Tasks;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Shared.Extensions;

namespace SecureFolderFS.Sdk.Extensions
{
    public static class NavigationExtensions
    {
        public static T? TryGetView<T>(this INavigationService navigationService)
            where T : class, IViewDesignation
        {
            return (T?)navigationService.Views.FirstOrDefault(x => x is T);
        }

        public static async Task<bool> TryNavigateAsync<T>(this INavigationService navigation, Func<T>? initializer = null, bool useInitialization = true)
            where T : class, IViewDesignation
        {
            var view = navigation.TryGetView<T>();
            var isNewView = view is null;

            view ??= initializer?.Invoke() ?? null;
            if (view is null)
                return false;

            // Initialize if the target supports IAsyncInitialize and doesn't already exist
            if (isNewView && useInitialization && view is IAsyncInitialize supportsAsyncInitialize)
                _ = supportsAsyncInitialize.InitAsync();

            // Navigate to the target
            return await navigation.NavigateAsync(view);
        }

        public static async Task<bool> ForgetNavigateCurrentViewAsync(this INavigationService navigationService, IViewDesignation view)
        {
            return await ForgetNavigateViewAsync(navigationService, view, () =>
            {
                if (navigationService.CurrentView is null)
                    return null;

                navigationService.Views.Remove(navigationService.CurrentView);
                return navigationService.CurrentView;
            });
        }

        public static async Task<bool> ForgetNavigateSpecificViewAsync(this INavigationService navigationService, IViewDesignation view, Func<IViewDesignation, bool> viewForgetter, bool addViewIfMissing = false)
        {
            return await ForgetNavigateViewAsync(navigationService, view, () =>
            {
                var targetViews = navigationService.Views.Where(viewForgetter).ToArray();
                if (targetViews.IsEmpty())
                    return null;

                // Remove all items
                foreach (var item in targetViews)
                {
                    (item as IDisposable)?.Dispose();
                    navigationService.Views.Remove(item);
                }

                // Return the first view of the removed views.
                // It doesn't matter which one it is, since ForgetNavigateViewAsync only replaces the removed view
                return targetViews.FirstOrDefault();
            }, navigationService.CurrentView is null || viewForgetter(navigationService.CurrentView), addViewIfMissing);
        }

        private static async Task<bool> ForgetNavigateViewAsync(this INavigationService navigationService, IViewDesignation view, Func<IViewDesignation?> viewForgetter, bool shouldTriggerNavigation = true, bool addViewIfMissing = false)
        {
            var navigated = false;
            IViewDesignation? currentView = null;

            try
            {
                currentView = viewForgetter();
                if (!shouldTriggerNavigation)
                {
                    // Silently replace the removed view without triggering navigation
                    if (currentView is not null || addViewIfMissing)
                        navigationService.Views.Add(view);

                    return false;
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
