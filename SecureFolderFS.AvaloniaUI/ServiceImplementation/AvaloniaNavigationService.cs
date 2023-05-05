﻿using SecureFolderFS.AvaloniaUI.Animations.Transitions.NavigationTransitions;
using SecureFolderFS.AvaloniaUI.UserControls.Navigation;
using SecureFolderFS.Sdk.Enums;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Shared.Extensions;
using SecureFolderFS.UI.ServiceImplementation;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Threading;

namespace SecureFolderFS.AvaloniaUI.ServiceImplementation
{
    /// <inheritdoc cref="INavigationService"/>
    internal sealed class AvaloniaNavigationService : BaseNavigationService<FrameNavigationControl>
    {
        /// <inheritdoc/>
        protected override async Task<bool> BeginNavigationAsync(INavigationTarget? target, NavigationType navigationType)
        {
            if (NavigationControl is null)
                return false;

            switch (navigationType)
            {
                case NavigationType.Backward:
                {
                    if (NavigationControl.ContentFrame.CanGoBack)
                    {
                        NavigationControl.ContentFrame.GoBack();

                        var contentType = NavigationControl.ContentFrame.Content?.GetType();
                        if (contentType is null)
                            return false;

                        var targetType = NavigationControl.TypeBinding.GetByKeyOrValue(contentType);
                        var backTarget = Targets.FirstOrDefault(x => x.GetType() == targetType);
                        if (backTarget is not null)
                            CurrentTarget = backTarget;

                        return true;
                    }

                    return false;
                }

                case NavigationType.Forward:
                    return false;

                default:
                case NavigationType.Detached:
                {
                    if (target is null)
                        return false;

                    return await (Dispatcher.UIThread.InvokeAsync(() => NavigationControl.NavigateAsync(target, (NavigationTransition?)null)).GetTask().Unwrap());
                }
            }
        }

        /// <inheritdoc/>
        public override void Dispose()
        {
            NavigationControl?.Dispose();
            base.Dispose();
        }
    }
}
