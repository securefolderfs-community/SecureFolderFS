using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using SecureFolderFS.Sdk.EventArguments;
using SecureFolderFS.Sdk.Extensions;
using SecureFolderFS.Sdk.ViewModels.Controls.Components;
using SecureFolderFS.Sdk.ViewModels.Views.Overlays;
using SecureFolderFS.Shared.Extensions;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace SecureFolderFS.Uno.UserControls.Introduction
{
    public sealed partial class FileSystemSlide : UserControl
    {
        public FileSystemSlide()
        {
            InitializeComponent();
        }

        private void FileSystems_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (OverlayViewModel is null)
                return;

            // Not installed yet - the Install button inside the item handles that flow
            if (e.ClickedItem is ItemInstallationViewModel { IsInstalled: false })
                return;

            // The view model synchronizes IsSelected across all items, which drives the selection visuals
            if (e.ClickedItem is PickerOptionViewModel itemViewModel)
                OverlayViewModel.SelectedFileSystem = itemViewModel;
        }

        private void FileSystems_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems is null)
                return;

            foreach (var installation in e.NewItems.OfType<ItemInstallationViewModel>())
                SubscribeInstallation(installation);
        }

        private void Installation_StateChanged(object? sender, EventArgs e)
        {
            if (e is not ErrorReportedEventArgs { Result.Successful: false } args)
                return;

            // Installation failures are actionable (the user can retry), so surface them in-flow
            InstallationErrorTip.Subtitle = args.Result.GetMessage("UnknownError".ToLocalized());
            InstallationErrorTip.IsOpen = true;
        }

        private void AttachInstallationHandlers(IntroductionOverlayViewModel? oldViewModel)
        {
            if (oldViewModel is not null)
                oldViewModel.FileSystems.CollectionChanged -= FileSystems_CollectionChanged;

            foreach (var installation in _subscribedInstallations)
                installation.StateChanged -= Installation_StateChanged;
            _subscribedInstallations.Clear();

            if (OverlayViewModel is null)
                return;

            // The list may still be initializing, so watch for late-added items as well
            OverlayViewModel.FileSystems.CollectionChanged += FileSystems_CollectionChanged;
            foreach (var installation in OverlayViewModel.FileSystems.OfType<ItemInstallationViewModel>())
                SubscribeInstallation(installation);
        }

        private void SubscribeInstallation(ItemInstallationViewModel installation)
        {
            installation.StateChanged += Installation_StateChanged;
            _subscribedInstallations.Add(installation);
        }

        private readonly List<ItemInstallationViewModel> _subscribedInstallations = new();

        public IntroductionOverlayViewModel? OverlayViewModel
        {
            get => (IntroductionOverlayViewModel?)GetValue(OverlayViewModelProperty);
            set => SetValue(OverlayViewModelProperty, value);
        }
        public static readonly DependencyProperty OverlayViewModelProperty =
            DependencyProperty.Register(nameof(OverlayViewModel), typeof(IntroductionOverlayViewModel), typeof(FileSystemSlide),
                new PropertyMetadata(null, static (sender, e) => ((FileSystemSlide)sender).AttachInstallationHandlers(e.OldValue as IntroductionOverlayViewModel)));
    }
}

