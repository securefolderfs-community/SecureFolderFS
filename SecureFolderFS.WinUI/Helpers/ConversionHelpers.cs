using Microsoft.UI.Xaml.Media.Animation;
using SecureFolderFS.Backend.Models.Transitions;

#nullable enable

namespace SecureFolderFS.WinUI.Helpers
{
    internal static class ConversionHelpers
    {
        public static NavigationTransitionInfo? ToNavigationTransitionInfo(TransitionModel? transitionModel)
        {
            return transitionModel switch
            {
                SlideTransitionModel slideTransitionModel => new SlideNavigationTransitionInfo() { Effect = (SlideNavigationTransitionEffect)slideTransitionModel.SlideTransitionDirection },
                SuppressTransitionModel => new SuppressNavigationTransitionInfo(),
                EntranceTransitionModel => new EntranceNavigationTransitionInfo(),
                DrillInTransitionModel => new DrillInNavigationTransitionInfo(),
                ContinuumTransitionModel => new ContinuumNavigationTransitionInfo(),
                _ => null
            };
        }
    }
}
