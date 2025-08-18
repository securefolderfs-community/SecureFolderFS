using CommunityToolkit.Maui.Views;
using SecureFolderFS.Maui.UserControls.Options;
using SecureFolderFS.Sdk.Extensions;
using SecureFolderFS.Sdk.ViewModels.Views.Credentials;
using SecureFolderFS.Sdk.ViewModels.Views.Overlays;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.UI.Utils;

namespace SecureFolderFS.Maui.Popups
{
    public partial class CredentialsPopup : Popup, IOverlayControl
    {
        public CredentialsPopup()
        {
            InitializeComponent();
        }

        /// <inheritdoc/>
        public async Task<IResult> ShowAsync()
        {
            if (ViewModel is null)
                return Shared.Models.Result.Failure(null);

            _ = await Shell.Current.CurrentPage.ShowPopupAsync(this);
            return Shared.Models.Result.Success;
        }

        /// <inheritdoc/>
        public void SetView(IViewable viewable)
        {
            ViewModel = (CredentialsOverlayViewModel)viewable;
        }

        /// <inheritdoc/>
        public Task HideAsync()
        {
            return CloseAsync();
        }

        private void AvailableOptionsPanel_Loaded(object? sender, EventArgs e)
        {
            if (ViewModel?.SelectionViewModel is not { } selectionViewModel || sender is not Layout layout)
                return;

            // Add "Modify existing" section
            if (selectionViewModel.ConfiguredViewModel is not null)
            {
                var itemsPanel = new VerticalStackLayout();
                var container = new OptionsContainer()
                {
                    Title = "ModifyExisting".ToLocalized(),
                    InnerContent = itemsPanel
                };

                // Modify existing
                itemsPanel.Children.Add(new OptionsControl()
                {
                    Title = selectionViewModel.ConfiguredViewModel.Title,
                    Command = selectionViewModel.ItemSelectedCommand
                });

                // Remove credentials
                if (selectionViewModel.CanRemoveCredentials)
                    itemsPanel.Children.Add(new OptionsControl()
                    {
                        Title = "RemoveAuthentication".ToLocalized(),
                        Command = selectionViewModel.RemoveCredentialsCommand
                    });

                if (itemsPanel.Children.LastOrDefault() is OptionsControl optionsControl)
                    optionsControl.IsSeparatorVisible = false;

                layout.Children.Insert(0, container);
            }

            // Add "All options" section
            var allOptionsSection = (layout.Children.Last() as OptionsContainer)?.InnerContent as Layout;
            if (allOptionsSection is null)
                return;

            // Add items to the options section
            // Note: We could hook up CollectionChanged event and listen for item
            //      changes, however, it'd be inefficient and unnecessary
            foreach (var item in selectionViewModel.AuthenticationOptions)
            {
                allOptionsSection.Add(new OptionsControl()
                {
                    Title = item.Title,
                    Command = selectionViewModel.ItemSelectedCommand,
                    CommandParameter = item
                });
            }

            if (allOptionsSection.LastOrDefault() is OptionsControl optionsControl2)
                optionsControl2.IsSeparatorVisible = false;
        }

        private async void ResetViewButton_Click(object? sender, EventArgs e)
        {
            if (ViewModel?.SelectedViewModel is not CredentialsResetViewModel credentialsResetViewModel)
                return;

            try
            {
                await credentialsResetViewModel.ConfirmAsync(default);
                await HideAsync();
            }
            catch (Exception ex)
            {
                // TODO: Report to user
                _ = ex;
            }    
        }

        private async void ConfirmationViewButton_Click(object? sender, EventArgs e)
        {
            if (ViewModel?.SelectedViewModel is not CredentialsConfirmationViewModel credentialsConfirmation)
                return;

            try
            {
                await credentialsConfirmation.ConfirmAsync(default);
                await HideAsync();
            }
            catch (Exception ex)
            {
                // TODO: Report to user
                _ = ex;
            }
        }

        public CredentialsOverlayViewModel? ViewModel
        {
            get => (CredentialsOverlayViewModel?)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }
        public static readonly BindableProperty ViewModelProperty =
            BindableProperty.Create(nameof(ViewModel), typeof(CredentialsOverlayViewModel), typeof(CredentialsPopup), null);
    }
}

