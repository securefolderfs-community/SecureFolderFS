using CommunityToolkit.Maui.Views;
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

        private void TableRoot_Loaded(object? sender, EventArgs e)
        {
            if (ViewModel?.SelectionViewModel is not { } selectionViewModel || sender is not TableView tableView)
                return;

            // Init "Modify existing" section
            if (selectionViewModel.ConfiguredViewModel is not null)
            {
                var modifyExistingSection = new TableSection()
                {
                    Title = "ModifyExisting".ToLocalized()
                };

                // Modify existing
                modifyExistingSection.Add(new TextCell()
                {
                    Text = selectionViewModel.ConfiguredViewModel.Title,
                    Detail = "ChangeCurrentAuthentication".ToLocalized(),
                    Command = selectionViewModel.ItemSelectedCommand
                });

                // Remove credentials
                if (selectionViewModel.CanRemoveCredentials)
                    modifyExistingSection.Add(new TextCell()
                    {
                        Text = "RemoveAuthentication".ToLocalized(),
                        Detail = "RemoveAuthenticationDescription".ToLocalized(),
                        Command = selectionViewModel.RemoveCredentialsCommand
                    });

                tableView.Root.Add(modifyExistingSection);
            }

            // Init "All options" section
            var allOptionsSection = tableView.Root.Last();

            // Add items to the options section
            // Note: We could hook up CollectionChanged event and listen for item
            //      changes, however, it'd be inefficient and unnecessary
            foreach (var item in selectionViewModel.AuthenticationOptions)
            {
                allOptionsSection.Add(new TextCell()
                {
                    Text = item.Title,
                    Detail = item.Description,
                    Command = selectionViewModel.ItemSelectedCommand,
                    CommandParameter = item
                });
            }
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

