using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.ViewModels.Views;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.Extensions
{
    public static class NavigationExtensions
    {
        public static T? TryGetTarget<T>(this INavigationService navigationService)
            where T : class, INavigationTarget
        {
            return (T?)navigationService.Targets.FirstOrDefault(x => x is T);
        }

        public static async Task<bool> TryNavigateAsync<T>(this INavigationService navigationService, Func<T>? initializer = null)
            where T : class, INavigationTarget
        {
            var target = navigationService.TryGetTarget<T>() ?? (initializer?.Invoke() ?? null);
            if (target is null)
                return false;

            return await navigationService.NavigateAsync(target);
        }

        public static async Task<bool> TryNavigateAndForgetAsync(this INavigationService navigationService, INavigationTarget target)
        {
            if (navigationService.CurrentTarget is not null)
                navigationService.Targets.Remove(navigationService.CurrentTarget);

            return await navigationService.NavigateAsync(target);
        }
    }
}
