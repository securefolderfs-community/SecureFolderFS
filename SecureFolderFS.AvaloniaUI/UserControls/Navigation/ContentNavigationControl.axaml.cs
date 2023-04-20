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
            var page = _backStack.Pop();
            _ = SetContentAsync(page.Item1, page.Item2, new SlideNavigationTransition(SlideNavigationTransition.Side.Left, ContentPresenter.Bounds.Width, true));
        }
        

        public virtual void Navigate<TViewModel>(TViewModel viewModel, NavigationTransition? transition)
            where TViewModel : class, INotifyPropertyChanged
        {
        }

        public void Navigate(Type pageType, object parameter, NavigationTransition? transition)
        {
            if (_currentPage is not null)
                _backStack.Push(_currentPage.Value);

            _ = SetContentAsync(pageType, parameter, transition);
        }

        public async Task SetContentAsync(Type pageType, object parameter, NavigationTransition? transition)
        {
            if (CurrentContent is Page currentPage)
                currentPage.OnNavigatingFrom();

            await Task.Delay(50);

            Task? oldContentAnimationTask = null;
            if (transition is not null)
                oldContentAnimationTask = transition.AnimateOldContentAsync(ContentPresenter);

            // TODO Caching
            var instance = Activator.CreateInstance(pageType);

            if (instance is Page page)
                page.OnNavigatedTo(new NavigationEventArgs(instance, NavigationMode.New, transition, parameter, pageType));

            if (oldContentAnimationTask is not null)
                await oldContentAnimationTask;

            CurrentContent = instance;
            _currentPage = new(pageType, parameter);

            if (transition is not null)
            {
                await transition.AnimateNewContentAsync(ContentPresenter);
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