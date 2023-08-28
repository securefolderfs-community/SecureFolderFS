using CommunityToolkit.Mvvm.Input;
using SecureFolderFS.Shared.Utilities;
using System;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.ViewModels.Views.Vault.Login
{
    public sealed partial class MigrationViewModel : BaseLoginViewModel
    {
        /// <inheritdoc/>
        public override event EventHandler<EventArgs>? StateChanged;

        /// <inheritdoc/>
        protected override void SetError(IResult? result)
        {
            throw new NotImplementedException();
        }

        [RelayCommand]
        private async Task MigrateAsync()
        {

        }
    }
}
