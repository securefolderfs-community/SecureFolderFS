using System;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using SecureFolderFS.Uno.Views.DebugViews;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace SecureFolderFS.Uno.UserControls.InterfaceRoot
{
    public sealed partial class DebugWindowRootControl : UserControl
    {
        public DebugWindowRootControl()
        {
            InitializeComponent();
        }

        private void NavigationView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            var tag = Convert.ToInt32((args.SelectedItem as NavigationViewItem)?.Tag);
            var pageType = tag switch
            {
                0 => typeof(DebugAppControlPage),
                1 => typeof(DebugFileSystemLogPage),
                _ => throw new ArgumentOutOfRangeException(nameof(tag))
            };

            Navigation.Navigate(pageType, null, new EntranceNavigationTransitionInfo());
        }
    }
}
