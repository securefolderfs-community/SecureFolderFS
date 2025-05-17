using OwlCore.Storage;
using SecureFolderFS.Shared.ComponentModel;

namespace SecureFolderFS.Sdk.ViewModels.Controls.Previewers
{
    /// <inheritdoc cref="BasePreviewerViewModel{TSource}"/>
    public abstract class FilePreviewerViewModel<TSource> : BasePreviewerViewModel<TSource>, IWrapper<IFile>
        where TSource : class
    {
        /// <inheritdoc/>
        public IFile Inner { get; }

        protected FilePreviewerViewModel(IFile inner)
        {
            Inner = inner;
        }
    }
}
