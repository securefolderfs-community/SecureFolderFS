﻿using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.UI.Utils;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SecureFolderFS.UI.ServiceImplementation
{
    /// <inheritdoc cref="IOverlayService"/>
    public abstract class BaseOverlayService : IOverlayService
    {
        protected Stack<IOverlayControl> Overlays { get; } = new();

        /// <inheritdoc/>
        public virtual async Task<IResult> ShowAsync(IView view)
        {
            var overlay = GetOverlay(view);
            overlay.SetView(view);
            Overlays.Push(overlay);

            // Show overlay
            var result = await overlay.ShowAsync();

            Overlays.Pop();
            return result;
        }

        protected abstract IOverlayControl GetOverlay(IView view);
    }
}
