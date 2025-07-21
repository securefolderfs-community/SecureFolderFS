using System;
using System.Threading.Tasks;
using System.Windows.Input;
using SecureFolderFS.Shared.ComponentModel;

namespace SecureFolderFS.UI.Utils
{
    public interface IMigratorControl : IProgress<IResult>, IDisposable
    {
        /// <summary>
        /// Gets or sets the command that signals the migrator to proceed to the migration stage.
        /// </summary>
        ICommand? MigrateCommand { get; set; }

        /// <summary>
        /// Continues the authentication process.
        /// </summary>
        /// <remarks>
        /// The implementor is responsible for invoking the <see cref="MigrateCommand"/> when authentication is finalized.
        /// </remarks>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
        Task ContinueAsync();
    }
}
