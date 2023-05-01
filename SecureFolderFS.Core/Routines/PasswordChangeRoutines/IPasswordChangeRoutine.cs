using SecureFolderFS.Shared.Utils;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Core.Routines.PasswordChangeRoutines
{
    public interface IPasswordChangeRoutine : IDisposable
    {
        Task SetKeystoreAsync(Stream keystoreStream, IAsyncSerializer<Stream> serializer, CancellationToken cancellationToken);

        void SetPassword(IPassword existingPassword, IPassword newPassword);

        Task WriteKeystoreAsync(Stream keystoreStream, IAsyncSerializer<Stream> serializer, CancellationToken cancellationToken);

        Task WriteConfigurationAsync(Stream configStream, IAsyncSerializer<Stream> serializer, CancellationToken cancellationToken);
    }
}
