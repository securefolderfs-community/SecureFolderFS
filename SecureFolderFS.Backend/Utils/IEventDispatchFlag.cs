namespace SecureFolderFS.Backend.Utils
{
    /// <summary>
    /// Provides dispatch for notifying the caller whether the event should be forwarded or not.
    /// </summary>
    public interface IEventDispatchFlag
    {
        /// <summary>
        /// Prevents forwarding of the event.
        /// </summary>
        void NoForwarding();
    }
}
