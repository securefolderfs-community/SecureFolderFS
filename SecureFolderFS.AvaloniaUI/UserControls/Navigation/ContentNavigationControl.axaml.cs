using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using FluentAvalonia.UI.Navigation;
using SecureFolderFS.AvaloniaUI.Animations.Transitions.NavigationTransitions;
using SecureFolderFS.Shared.Extensions;
using SecureFolderFS.UI.Controls;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NavigationEventArgs = SecureFolderFS.AvaloniaUI.Events.NavigationEventArgs;

namespace SecureFolderFS.AvaloniaUI.UserControls.Navigation
{
    /// <summary>
    /// The base class that manages UI navigation.
    /// </summary>
    internal abstract partial class ContentNavigationControl : UserControl, INavigationControl
    {
        protected (Type, object)? currentPage;
        protected readonly Stack<(Type, object)> backStack;

        /// <summary>
        /// Gets a dictionary of types that bind view models and pages together.
        /// </summary>
        public abstract Dictionary<Type, Type> TypeBinding { get; }

        /// <summary>
        /// Gets the value that determines whether back navigation is available.
        /// </summary>
        public bool CanGoBack => !backStack.IsEmpty();

        public ContentNavigationControl()
        {
            backStack = new();
            AvaloniaXamlLoader.Load(this);
        }

        /// <summary>
        /// Navigates back.
        /// </summary>
        public virtual void GoBack()
        {
            if (!CanGoBack)
                return;

            var page = backStack.Pop();
            _ = SetContentAsync(page.Item1, page.Item2, new SlideNavigationTransition(SlideNavigationTransition.Side.Left, Presenter.Bounds.Width, true));
        }

        /// <summary>
        /// Navigates forward.
        /// </summary>
        public virtual void GoForward()
        {
            // TODO: Navigate forward
            throw new NotImplementedException();
        }
        
        /// <inheritdoc/>
        public async Task<bool> NavigateAsync<TTarget, TTransition>(TTarget? target, TTransition? transition = null)
            where TTransition : class
        {
            if (target is null)
            {
                CurrentContent = null;
                return true;
            }

            var pageType = TypeBinding.GetByKeyOrValue(target.GetType());
            if (pageType is null)
                return false;

            return await NavigateContentAsync(pageType, target, transition as NavigationTransition);
        }

        /// <summary>
        /// Navigates a frame to specified <paramref name="pageType"/>.
        /// </summary>
        /// <param name="pageType">The type of page to navigate to.</param>
        /// <param name="parameter">The parameter to pass to the page.</param>
        /// <param name="transition">The transition to use when navigating.</param>
        /// <returns>If successful, returns true, otherwise false.</returns>
        protected abstract Task<bool> NavigateContentAsync(Type pageType, object parameter, NavigationTransition? transition);

        protected async Task<bool> SetContentAsync(Type pageType, object parameter, NavigationTransition? transition)
        {
            if (CurrentContent is Page current)
                current.OnNavigatingFrom();

            await Task.Delay(50);

            Task? oldContentAnimationTask = null;
            if (transition is not null)
                oldContentAnimationTask = transition.AnimateOldContentAsync(Presenter);

            // TODO Caching
            var instance = Activator.CreateInstance(pageType);

            if (instance is Page page)
                page.OnNavigatedTo(new NavigationEventArgs(instance, NavigationMode.New, transition, parameter, pageType));

            if (oldContentAnimationTask is not null)
                await oldContentAnimationTask;

            CurrentContent = instance;
            currentPage = new(pageType, parameter);

            if (transition is not null)
                await transition.AnimateNewContentAsync(Presenter);

            return true;
        }

        /// <inheritdoc/>
        public virtual void Dispose()
        {
            (CurrentContent as IDisposable)?.Dispose();
        }

        public object? CurrentContent
        {
            get => GetValue(CurrentContentProperty);
            set => SetValue(CurrentContentProperty, value);
        }
        public static readonly StyledProperty<object?> CurrentContentProperty
            = AvaloniaProperty.Register<ContentNavigationControl, object?>(nameof(CurrentContent));
    }
}