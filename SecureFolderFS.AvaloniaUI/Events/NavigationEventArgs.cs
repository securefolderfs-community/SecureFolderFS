using System;
using FluentAvalonia.UI.Navigation;
using SecureFolderFS.AvaloniaUI.Animations.Transitions.NavigationTransitions;

namespace SecureFolderFS.AvaloniaUI.Events
{
    internal sealed class NavigationEventArgs
    {
        public NavigationEventArgs(object content, NavigationMode mode, NavigationTransition? transition, object param, Type srcPgType)
        {
            Content = content;
            NavigationMode = mode;
            NavigationTransition = transition;
            Parameter = param;
            SourcePageType = srcPgType;
        }

        /// <summary>
        /// Gets the root node of the target page's content.
        /// </summary>
        public object Content { get; }

        /// <summary>
        /// Gets a value that indicates the direction of movement during navigation
        /// </summary>
        public NavigationMode NavigationMode { get; }

        /// <summary>
        /// Gets any "Parameter" object passed to the target page for the navigation.
        /// </summary>
        public object Parameter { get; }

        /// <summary>
        /// Gets the data type of the source page.
        /// </summary>
        public Type SourcePageType { get; }

        /// <summary>
        /// Gets a value that indicates the animated transition associated with the navigation.
        /// </summary>
        public NavigationTransition? NavigationTransition { get; }
    }
}