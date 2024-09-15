using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using SecureFolderFS.Sdk.EventArguments;
using SecureFolderFS.Shared.ComponentModel;

namespace SecureFolderFS.UI.ViewModels
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

        public PasswordLoginViewModel(string id)
            : base(id)
        {
        }

        public override void Report(IResult? result)
        {
            base.Report(result);
            IsPasswordInvalid = !result?.Successful ?? false;
        }

        /// <inheritdoc/>
        protected override Task ProvideCredentialsAsync(CancellationToken cancellationToken)
        {
            var key = TryGetPasswordAsKey();
            if (key is not null)
                CredentialsProvided?.Invoke(this, new(key));

            return Task.CompletedTask;
        }
    }
}
