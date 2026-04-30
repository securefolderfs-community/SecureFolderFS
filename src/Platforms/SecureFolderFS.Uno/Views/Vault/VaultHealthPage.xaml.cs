using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.System;
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
            if (sender is not TextBlock textBlock)
                return;

            HealthNameIssueViewModel? viewModel = null;
            if (textBlock.DataContext is ActionBlockControl { DataContext: HealthNameIssueViewModel actionBlockViewModel })
                viewModel = actionBlockViewModel;

            if (viewModel is null && textBlock is { DataContext: HealthNameIssueViewModel dataContextViewModel })
                viewModel = dataContextViewModel;

            if (viewModel is null)
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

        private async void NameEdit_IsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            // Workaround: Since Visibility does not have *Changed event,
            // we rely on IsEnabled to notify when the IsEditing property changes and set the focus
            if (e.NewValue is not true)
                return;

            if (sender is not TextBox textBox)
                return;

            if (textBox.DataContext is not ActionBlockControl { DataContext: HealthNameIssueViewModel } && textBox is not { DataContext: HealthNameIssueViewModel })
                return;

            await Task.Delay(100);
            textBox.Focus(FocusState.Programmatic);
            textBox.SelectAll();
        }

        private void NameEdit_PreviewKeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (ViewModel is null)
                return;

            if (sender is not TextBox textBox)
                return;

            ActionBlockControl? actionBlock = null;
            if (textBox.DataContext is ActionBlockControl dataContextActionBlock)
                actionBlock = dataContextActionBlock;

            if (actionBlock is null && textBox.FindAscendant<ActionBlockControl>() is { } foundActionBlock)
                actionBlock = foundActionBlock;

            if (actionBlock?.DataContext is not HealthNameIssueViewModel viewModel)
                return;

            switch (e.Key)
            {
                case VirtualKey.Escape:
                {
                    e.Handled = true;
                    viewModel.IsEditing = false;
                    break;
                }

                case VirtualKey.Enter:
                {
                    e.Handled = true;
                    viewModel.IsEditing = false;

                    ItemsControl itemsList;
                    IList<HealthIssueViewModel> issueCollection;

                    var listViewAscendant = actionBlock.FindAscendant<ListView>();
                    if (listViewAscendant is { DataContext: ActionBlockControl { DataContext: HealthDirectoryIssueViewModel directoryIssue } } parentListView)
                    {
                        itemsList = parentListView;
                        issueCollection = directoryIssue.Issues;
                    }
                    else
                    {
                        itemsList = ItemsList;
                        issueCollection = ViewModel.HealthViewModel.FoundIssues;
                    }

                    var nextIndex = issueCollection.IndexOf(viewModel) + 1;
                    if (issueCollection.Count == 1)
                        return;

                    if (nextIndex >= issueCollection.Count)
                        nextIndex = 0;

                    if (issueCollection.ElementAtOrDefault(nextIndex) is not HealthNameIssueViewModel nextItem)
                        return;

                    if (itemsList.ContainerFromIndex(nextIndex) is not ContentControl nextContainer)
                        return;

                    var contentTemplateRoot = nextContainer.GetContentControlRoot();
                    if (contentTemplateRoot is not ActionBlockControl nextActionBlock || nextActionBlock.FindDescendant<TextBox>() is not { } textBox2)
                        return;

                    nextItem.IsEditing = true;
                    textBox2.Focus(FocusState.Programmatic);

                    break;
                }
            }
        }
    }
}
