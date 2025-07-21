using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using SecureFolderFS.Sdk.EventArguments;
using SecureFolderFS.Shared.ComponentModel;

namespace SecureFolderFS.UI.ViewModels.Authentication
{
    /// <inheritdoc cref="PasswordLoginViewModel"/>
    [Bindable(true)]
    public sealed partial class PasswordLoginViewModel : PasswordViewModel
    {
        [ObservableProperty] private bool _IsPasswordInvalid;

        /// <inheritdoc/>
        public override event EventHandler<EventArgs>? StateChanged;

        /// <inheritdoc/>
        public override event EventHandler<CredentialsProvidedEventArgs>? CredentialsProvided;

        public override void Report(IResult? result)
        {
            base.Report(result);
            IsPasswordInvalid = !result?.Successful ?? false;
        }

        /// <inheritdoc/>
        protected override async Task ProvideCredentialsAsync(CancellationToken cancellationToken)
        {
            var key = TryGetPasswordAsKey();
            if (key is null)
                return;

            var tcs = new TaskCompletionSource();
            CredentialsProvided?.Invoke(this, new(key, tcs));
            await tcs.Task;
        }
    }
}
