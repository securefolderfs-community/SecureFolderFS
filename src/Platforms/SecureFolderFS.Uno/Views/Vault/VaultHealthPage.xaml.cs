using System.Collections.Generic;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.WinUI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Navigation;
using SecureFolderFS.Sdk.ViewModels.Controls.Widgets.Health;
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
            if (sender is not FrameworkElement { DataContext: ActionBlockControl { DataContext: HealthNameIssueViewModel viewModel }} textBlock)
                return;

            viewModel.IsEditing = true;
            var textBox = textBlock.GetParent<Grid>()?.Children.FirstOrDefaultType<UIElement, TextBox>();
            textBox?.Focus(FocusState.Programmatic);
            textBox?.SelectAll();
        }

        private void NameEdit_LostFocus(object sender, RoutedEventArgs e)
        {
            if (sender is not FrameworkElement { DataContext: ActionBlockControl { DataContext: HealthNameIssueViewModel viewModel } })
                return;

            viewModel.IsEditing = false;
        }

        private void NameEdit_IsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            // Workaround: Since Visibility does not have *Changed event,
            // we rely on IsEnabled to notify when the IsEditing property changes and set the focus
            if (sender is not TextBox { DataContext: ActionBlockControl { DataContext: HealthNameIssueViewModel } } textBox)
                return;

            if (e.NewValue is not true)
                return;

            textBox.Focus(FocusState.Programmatic);
            textBox.SelectAll();
        }

        private void NameEdit_PreviewKeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (ViewModel is null)
                return;

            if (sender is not FrameworkElement { DataContext: ActionBlockControl { DataContext: HealthNameIssueViewModel nameIssueViewModel } actionBlock })
                return;

            switch (e.Key)
            {
                case VirtualKey.Escape:
                {
                    e.Handled = true;
                    nameIssueViewModel.IsEditing = false;
                    break;
                }

                case VirtualKey.Enter:
                {
                    e.Handled = true;
                    nameIssueViewModel.IsEditing = false;

                    ItemsControl itemsList;
                    IList<HealthIssueViewModel> issueCollection;

                    if (actionBlock.FindAscendant<ListView>() is { DataContext: ActionBlockControl { DataContext: HealthDirectoryIssueViewModel directoryIssue } } parentListView)
                    {
                        itemsList = parentListView;
                        issueCollection = directoryIssue.Issues;
                    }
                    else
                    {
                        itemsList = ItemsList;
                        issueCollection = ViewModel.HealthViewModel.FoundIssues;
                    }

                    var nextIndex = issueCollection.IndexOf(nameIssueViewModel) + 1;
                    if (issueCollection.Count == 1)
                        return;

                    if (nextIndex >= issueCollection.Count)
                        nextIndex = 0;

                    if (issueCollection.ElementAtOrDefault(nextIndex) is not HealthNameIssueViewModel nextItem)
                        return;

                    var nextContainer = itemsList.ContainerFromIndex(nextIndex);
                    if (nextContainer is not ContentControl { ContentTemplateRoot: ActionBlockControl nextActionBlock } || nextActionBlock.FindDescendant<TextBox>() is not { } textBox)
                        return;

                    nextItem.IsEditing = true;
                    textBox.Focus(FocusState.Programmatic);

                    break;
                }
            }
        }
    }
}
