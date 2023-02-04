using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using CommunityToolkit.Mvvm.Messaging;
using FluentAvalonia.UI.Navigation;
using SecureFolderFS.AvaloniaUI.Animations.Transitions;
using SecureFolderFS.AvaloniaUI.Animations.Transitions.NavigationTransitions;
using SecureFolderFS.Sdk.Messages.Navigation;
using SecureFolderFS.Shared.Extensions;
using NavigationEventArgs = SecureFolderFS.AvaloniaUI.Events.NavigationEventArgs;

namespace SecureFolderFS.AvaloniaUI.UserControls.Navigation
{
    /// <summary>
    /// The base class to manage navigation of pages using messages.
    /// </summary>
    internal partial class NavigationControl : UserControl, IDisposable, IRecipient<NavigationRequestedMessage>, IRecipient<BackNavigationRequestedMessage>
    {
        private readonly Stack<(Type, object)> _backStack;

        public bool CanGoBack => !_backStack.IsEmpty();

        private (Type, object)? _currentPage;

        public NavigationControl()
        {
            _backStack = new();

            InitializeComponent();
        }

        public void GoBack()
        {
            var page = _backStack.Pop();
            _ = SetContentAsync(page.Item1, page.Item2, new SlideNavigationTransition(SlideNavigationTransition.Side.Left, ContentPresenter.Bounds.Width, true));
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        /// <inheritoc/>
        public virtual void Receive(NavigationRequestedMessage message)
        {
            Navigate(message.ViewModel, null);
        }

        /// <inheritoc/>
        public virtual void Receive(BackNavigationRequestedMessage message)
        {
            _ = message;
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

        public static readonly StyledProperty<object?> CurrentContentProperty
            = AvaloniaProperty.Register<NavigationControl, object?>(nameof(CurrentContent));

        public object? CurrentContent
        {
            get => GetValue(CurrentContentProperty);
            set => SetValue(CurrentContentProperty, value);
        }
    }
}