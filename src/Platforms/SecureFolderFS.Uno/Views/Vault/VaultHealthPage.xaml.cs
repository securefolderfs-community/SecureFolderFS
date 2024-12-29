using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Navigation;
using SecureFolderFS.Sdk.ViewModels.Views.Vault;
using SecureFolderFS.Shared.Extensions;
using SecureFolderFS.UI.ViewModels.Health;
using SecureFolderFS.Uno.Extensions;
using SecureFolderFS.Uno.UserControls.ActionBlocks;
using Windows.System;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace SecureFolderFS.Uno.Views.Vault
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    [INotifyPropertyChanged]
    public sealed partial class VaultHealthPage : Page
    {
        public VaultHealthReportViewModel? ViewModel
        {
            get => DataContext.TryCast<VaultHealthReportViewModel>();
            set { DataContext = value; OnPropertyChanged(); }
        }

        public VaultHealthPage()
        {
            InitializeComponent();
        }

        /// <inheritdoc/>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter is VaultHealthReportViewModel viewModel)
                ViewModel = viewModel;

            base.OnNavigatedTo(e);
        }

        private void NameText_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (sender is not FrameworkElement { DataContext: ActionBlockControl { DataContext: HealthRenameIssueViewModel viewModel }} textBlock)
                return;

            viewModel.IsEditing = true;
            var textBox = textBlock.GetParent<Grid>()?.Children.FirstOrDefaultType<UIElement, TextBox>();
            textBox?.Focus(FocusState.Programmatic);
            textBox?.SelectAll();
        }

        private void NameEdit_LostFocus(object sender, RoutedEventArgs e)
        {
            if (sender is not FrameworkElement { DataContext: ActionBlockControl { DataContext: HealthRenameIssueViewModel viewModel } })
                return;

            viewModel.IsEditing = false;
        }

        private void NameEdit_IsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            // Workaround: Since Visibility does not have *Changed event,
            // we rely on IsEnabled to notify when the IsEditing property changes and set the focus
            if (sender is not TextBox { DataContext: ActionBlockControl { DataContext: HealthRenameIssueViewModel } } textBox)
                return;

            if (e.NewValue is not true)
                return;

            textBox.Focus(FocusState.Programmatic);
            textBox.SelectAll();
        }

        private void NameEdit_PreviewKeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (sender is not FrameworkElement { DataContext: ActionBlockControl { DataContext: HealthRenameIssueViewModel viewModel } actionBlock })
                return;

            switch (e.Key)
            {
                case VirtualKey.Enter:
                case VirtualKey.Escape:
                    e.Handled = true;
                    viewModel.IsEditing = false;
                    break;

                case VirtualKey.Tab:
                    if (actionBlock.GetParent<ListView>()?.DataContext is not ActionBlockControl { DataContext: HealthCascadingIssuesViewModel cascadingViewModel })
                        break;

                    e.Handled = true;
                    viewModel.IsEditing = false;

                    var index = cascadingViewModel.Issues.IndexOf(viewModel) + 1;
                    if (index >= cascadingViewModel.Issues.Count)
                        index = 0;

                    if (cascadingViewModel.Issues.ElementAtOrDefault(index) is HealthRenameIssueViewModel nextElement)
                        nextElement.IsEditing = true;

                    break;
            }
        }
    }
}
