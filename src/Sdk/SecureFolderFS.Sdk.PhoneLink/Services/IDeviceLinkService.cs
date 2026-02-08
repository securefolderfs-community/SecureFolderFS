using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SecureFolderFS.Sdk.PhoneLink.Models;
using SecureFolderFS.Sdk.PhoneLink.ViewModels;

namespace SecureFolderFS.Sdk.PhoneLink.Services
{
    public interface IDeviceLinkService : IDisposable
    {
        event EventHandler<CredentialViewModel>? EnrollmentCompleted;
        event EventHandler<PairingRequestViewModel>? PairingRequested;
        event EventHandler<AuthenticationRequestModel>? AuthenticationRequested;
        event EventHandler<string>? VerificationCodeReady;
        event EventHandler? Disconnected;
        event EventHandler? AuthenticationCompleted;

        bool IsListening { get; }

        Task StartListeningAsync(CancellationToken cancellationToken = default);

        void StopListening();

        void ConfirmPairingRequest(bool value);
    }
}
