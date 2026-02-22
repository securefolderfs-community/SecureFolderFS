using System.Threading.Tasks;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.UI.Utils;

namespace SecureFolderFS.UI.ServiceImplementation
{
    /// <inheritdoc cref="IOverlayService"/>
    public abstract class BaseOverlayService : IOverlayService
    {
        private IOverlayControl? _currentOverlay;
        
        /// <inheritdoc/>
        public IViewable? CurrentView { get; protected set; }

        /// <inheritdoc/>
        public virtual async Task<IResult> ShowAsync(IViewable viewable)
        {
            var previousView = CurrentView;

            try
            {
                _currentOverlay = GetOverlay(viewable);
                _currentOverlay.SetView(viewable);
                CurrentView = viewable;

                // Show overlay
                return await _currentOverlay.ShowAsync();
            }
            finally
            {
                CurrentView = previousView;
                _currentOverlay = null;
            }
        }

        /// <inheritdoc/>
        public virtual async Task CloseAllAsync()
        {
            if (_currentOverlay is not null)
                await _currentOverlay.HideAsync();
        }

        protected abstract IOverlayControl GetOverlay(IViewable viewable);
    }
}
