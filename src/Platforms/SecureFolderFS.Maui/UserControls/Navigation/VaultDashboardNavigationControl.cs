namespace SecureFolderFS.Maui.UserControls.Navigation
{
    internal sealed class VaultDashboardNavigationControl : ContentNavigationControl
    {
        /// <inheritdoc/>
        protected override Task<IAsyncDisposable?> ApplyTransitionAsync<TTarget, TTransition>(TTarget? target, TTransition? transition = default)
            where TTarget : default
            where TTransition : class
        {
            return Task.FromResult<IAsyncDisposable?>(null);
        }
    }
}
