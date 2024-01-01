using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SecureFolderFS.Sdk.EventArguments;
using SecureFolderFS.Sdk.Storage;
using SecureFolderFS.Shared.ComponentModel;
using System;

namespace SecureFolderFS.UI.ViewModels
{
    public sealed partial class PasswordLoginViewModel : PasswordViewModel
    {
        [ObservableProperty] private bool _IsPasswordInvalid;

        /// <inheritdoc/>
        public override event EventHandler<EventArgs>? StateChanged;

        public PasswordLoginViewModel(string id, IFolder vaultFolder)
            : base(id, vaultFolder)
        {
        }

        public override void SetError(IResult? result)
        {
            base.SetError(result);
            IsPasswordInvalid = !result?.Successful ?? false;
        }

        [RelayCommand]
        private void ProvideCredentials()
        {
            var key = RetrieveKey();
            if (key is null)
                return;

            StateChanged?.Invoke(this, new AuthenticationChangedEventArgs(key));
        }
    }
}
