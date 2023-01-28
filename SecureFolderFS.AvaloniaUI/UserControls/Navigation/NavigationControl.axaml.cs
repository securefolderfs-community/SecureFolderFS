using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;
using CommunityToolkit.Mvvm.Messaging;
using FluentAvalonia.UI.Controls;
using FluentAvalonia.UI.Media.Animation;
using FluentAvalonia.UI.Navigation;
using SecureFolderFS.Sdk.Messages.Navigation;
using SecureFolderFS.Shared.Extensions;

namespace SecureFolderFS.AvaloniaUI.UserControls.Navigation
{
    /// <summary>
    /// The base class to manage navigation of pages using messages.
    /// </summary>
    internal partial class NavigationControl : UserControl, IDisposable, IRecipient<NavigationRequestedMessage>, IRecipient<BackNavigationRequestedMessage>
    {
        private readonly Stack<(Type, object)> _backStack;

        public bool CanGoBack => !_backStack.IsEmpty();

        private ContentPresenter ContentPresenter => (ContentPresenter)this.GetVisualChildren().First();

        private (Type, object)? _currentPage;

        public NavigationControl()
        {
            _backStack = new();

            InitializeComponent();
        }

        public void GoBack()
        {
            var x = _backStack.Pop();
            SetContent(x.Item1, x.Item2, new SlideNavigationTransitionInfo());
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

        public virtual void Navigate<TViewModel>(TViewModel viewModel, NavigationTransitionInfo? transitionInfo)
            where TViewModel : class, INotifyPropertyChanged
        {
        }

        public async Task Navigate(Type pageType, object parameter, NavigationTransitionInfo? transitionInfo)
        {
            if (_currentPage is not null)
                _backStack.Push(_currentPage.Value);

            SetContent(pageType, parameter, transitionInfo);
        }

        public async Task SetContent(Type pageType, object parameter, NavigationTransitionInfo? transitionInfo)
        {
            if (Content is Page currentPage)
                currentPage.OnNavigatingFrom();
            await Task.Delay(50);

            // TODO Caching
            var instance = Activator.CreateInstance(pageType);
            await FadeOldContentAnimation();

            if (instance is Page page)
                page.OnNavigatedTo(new Events.NavigationEventArgs(instance, NavigationMode.New, transitionInfo, parameter, pageType));

            Content = instance;
            _currentPage = new(pageType, parameter);

            if (transitionInfo is SlideNavigationTransitionInfo)
            {
                ((TranslateTransform)ContentPresenter.RenderTransform!).X = 100d;
                await SlideInNewContentFromRightAnimation();
                return;
            }

            ((TranslateTransform)ContentPresenter.RenderTransform!).Y = 200d;
            await SlideInNewContentFromBottomAnimation();
        }

        /// <inheritdoc/>
        public virtual void Dispose()
        {
            (Content as IDisposable)?.Dispose();
        }

        private async Task FadeOldContentAnimation()
        {
            var animation = new Animation
            {
                Duration = TimeSpan.Parse("0:0:0:0.1"),
                FillMode = FillMode.Backward,
                Children =
                {
                    new KeyFrame
                    {
                        Cue = new(0),
                        Setters = { new Setter(OpacityProperty, 1d) }
                    },
                    new KeyFrame
                    {
                        Cue = new(1),
                        Setters = { new Setter(OpacityProperty, 0d) }
                    },
                }
            };
            await animation.RunAsync(ContentPresenter, null);
        }

        private async Task SlideInNewContentFromBottomAnimation()
        {
            var animation = new Animation
            {
                Duration = TimeSpan.Parse("0:0:0:0.3"),
                FillMode = FillMode.Forward,
                Easing = new CubicEaseOut(),
                Children =
                {
                    new KeyFrame
                    {
                        Cue = new(0),
                        Setters = { new Setter(TranslateTransform.YProperty, 200d) }
                    },
                    new KeyFrame
                    {
                        Cue = new(1),
                        Setters = { new Setter(TranslateTransform.YProperty, 0d) }
                    },
                }
            };
            await animation.RunAsync(ContentPresenter, null);
        }

        private Task SlideInNewContentFromRightAnimation()
        {
            var animation = new Animations.Animation
            {
                Duration = TimeSpan.Parse("0:0:0:0.3"),
                FillMode = FillMode.Forward,
                Easing = new CubicEaseOut(),
                Target = ContentPresenter,
                From = { new Setter(TranslateTransform.XProperty, 100d) },
                To = { new Setter(TranslateTransform.XProperty, 0d) }
            };
            return animation.RunAsync();
        }
    }
}