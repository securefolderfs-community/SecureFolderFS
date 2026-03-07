using System;
using System.Threading;
using System.Threading.Tasks;
using SecureFolderFS.Sdk.DeviceLink.Models;
using SecureFolderFS.Sdk.DeviceLink.ViewModels;

namespace SecureFolderFS.Sdk.DeviceLink.Services
{
    public interface IDeviceLinkService : IDisposable
    {
        event EventHandler<CredentialViewModel>? EnrollmentCompleted;
        event EventHandler<PairingRequestViewModel>? PairingRequested;
        event EventHandler<AuthenticationRequestViewModel>? AuthenticationRequested;
        event EventHandler<string>? VerificationCodeReady;
        event EventHandler? Disconnected;
        event EventHandler? AuthenticationCompleted;

        bool IsListening { get; }

        Task StartListeningAsync(CancellationToken cancellationToken = default);

        void StopListening();
    }
}
