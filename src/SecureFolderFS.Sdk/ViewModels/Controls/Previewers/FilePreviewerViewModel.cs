using OwlCore.Storage;
using SecureFolderFS.Shared.ComponentModel;

namespace SecureFolderFS.Sdk.ViewModels.Controls.Previewers
{
    /// <inheritdoc cref="BasePreviewerViewModel"/>
    public abstract class FilePreviewerViewModel : BasePreviewerViewModel, IWrapper<IFile>
    {
        /// <inheritdoc/>
        public IFile Inner { get; }

        protected FilePreviewerViewModel(IFile inner)
        {
            Inner = inner;
        }
    }
}
