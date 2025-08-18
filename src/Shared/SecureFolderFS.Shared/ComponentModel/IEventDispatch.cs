namespace SecureFolderFS.Shared.ComponentModel
{
    /// <summary>
    /// Provides dispatch for notifying the caller whether the event should be forwarded or not.
    /// </summary>
    public interface IEventDispatch
    {
        /// <summary>
        /// Prevents forwarding of the event.
        /// </summary>
        void PreventForwarding();
    }
}
