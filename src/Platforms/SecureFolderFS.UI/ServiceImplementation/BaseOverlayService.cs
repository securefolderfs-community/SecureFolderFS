using System.Threading.Tasks;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.UI.Utils;

namespace SecureFolderFS.UI.ServiceImplementation
{
    /// <inheritdoc cref="IOverlayService"/>
    public abstract class BaseOverlayService : IOverlayService
    {
        /// <inheritdoc/>
        public virtual async Task<IResult> ShowAsync(IViewable viewable)
        {
            var overlay = GetOverlay(viewable);
            overlay.SetView(viewable);

            // Show overlay
            return await overlay.ShowAsync();
        }

        protected abstract IOverlayControl GetOverlay(IViewable viewable);
    }
}
