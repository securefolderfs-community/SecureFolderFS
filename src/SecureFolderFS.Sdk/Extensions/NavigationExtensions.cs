using SecureFolderFS.Sdk.Services;
using System;
using System.Linq;
using System.Threading.Tasks;
using SecureFolderFS.Shared.ComponentModel;

namespace SecureFolderFS.Sdk.Extensions
{
    public static class NavigationExtensions
    {
        public static T? TryGetTarget<T>(this INavigationService navigationService)
            where T : class, INavigationTarget
        {
            return (T?)navigationService.Targets.FirstOrDefault(x => x is T);
        }

        public static async Task<bool> TryNavigateAsync<T>(this INavigationService navigationService, Func<T>? initializer = null, bool useInitialization = true)
            where T : class, INavigationTarget
        {
            var target = navigationService.TryGetTarget<T>();
            var isNewTarget = target is null;

            target ??= initializer?.Invoke() ?? null;
            if (target is null)
                return false;

            // Initialize if the target supports IAsyncInitialize and doesn't already exist
            if (isNewTarget && useInitialization && target is IAsyncInitialize supportsAsyncInitialize)
                _ = supportsAsyncInitialize.InitAsync();

            // Navigate to the target
            return await navigationService.NavigateAsync(target);
        }

        public static async Task<bool> TryNavigateAndForgetAsync(this INavigationService navigationService, INavigationTarget target)
        {
            var navigated = false;
            INavigationTarget? currentTarget = null;

            try
            {
                if (navigationService.CurrentTarget is not null)
                {
                    navigationService.Targets.Remove(navigationService.CurrentTarget);
                    currentTarget = navigationService.CurrentTarget;
                }

                navigated = await navigationService.NavigateAsync(target);
                return navigated;
            }
            finally
            {
                if (navigated)
                    (currentTarget as IDisposable)?.Dispose();
            }
        }
    }
}
