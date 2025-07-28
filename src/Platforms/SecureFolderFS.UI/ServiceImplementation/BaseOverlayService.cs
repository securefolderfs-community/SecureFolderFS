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
        public IViewable? CurrentView { get; protected set; }

        /// <inheritdoc/>
        public virtual async Task<IResult> ShowAsync(IViewable viewable)
        {
            var previousView = CurrentView;

            try
            {
                var overlay = GetOverlay(viewable);
                overlay.SetView(viewable);
                CurrentView = viewable;

                // Show overlay
                return await overlay.ShowAsync();
            }
            finally
            {
                CurrentView = previousView;
            }
        }

        protected abstract IOverlayControl GetOverlay(IViewable viewable);
    }
}
