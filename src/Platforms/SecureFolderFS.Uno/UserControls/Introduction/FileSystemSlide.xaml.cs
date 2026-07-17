using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
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

        public IntroductionOverlayViewModel? OverlayViewModel
        {
            get => (IntroductionOverlayViewModel?)GetValue(OverlayViewModelProperty);
            set => SetValue(OverlayViewModelProperty, value);
        }
        public static readonly DependencyProperty OverlayViewModelProperty =
            DependencyProperty.Register(nameof(OverlayViewModel), typeof(IntroductionOverlayViewModel), typeof(FileSystemSlide), new PropertyMetadata(null));
    }
}

