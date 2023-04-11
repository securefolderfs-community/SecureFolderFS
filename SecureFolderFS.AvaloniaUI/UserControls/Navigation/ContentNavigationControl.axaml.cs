using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using FluentAvalonia.UI.Navigation;
using SecureFolderFS.AvaloniaUI.Animations.Transitions;
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
        private (Type, object)? _currentPage;
        private readonly Stack<(Type, object)> _backStack;

        public bool CanGoBack => !_backStack.IsEmpty();

        public ContentNavigationControl()
        {
            _backStack = new();
            AvaloniaXamlLoader.Load(this);
        }

        public void GoBack()
        {
            var x = _backStack.Pop();
            SetContent(x.Item1, x.Item2, new SlideNavigationTransition(SlideNavigationTransition.Side.Left, SlideNavigationTransition.BigOffset));
        }

        /// <inheritdoc/>
        public abstract Task<bool> NavigateAsync<TTarget, TTransition>(TTarget target, TTransition? transition = default) where TTransition : class;

        public async Task Navigate(Type pageType, object parameter, TransitionBase? transition)
        {
            if (_currentPage is not null)
                _backStack.Push(_currentPage.Value);

            await SetContent(pageType, parameter, transition);
        }

        public async Task SetContent(Type pageType, object parameter, TransitionBase? transition)
        {
            if (CurrentContent is Page currentPage)
                currentPage.OnNavigatingFrom();

            await Task.Delay(50);

            // TODO: Implement Caching
            var instance = Activator.CreateInstance(pageType);
            if (transition is not SuppressNavigationTransition)
                await FadeOutContentStoryboard.RunAnimationsAsync();

            if (instance is Page page)
                page.OnNavigatedTo(new NavigationEventArgs(instance, NavigationMode.New, transition, parameter, pageType));

            CurrentContent = instance;
            _currentPage = new(pageType, parameter);

            if (transition is not null && transition is not SuppressNavigationTransition)
            {
                transition.Target = ContentPresenter;
                await transition.RunAnimationAsync();
            }
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