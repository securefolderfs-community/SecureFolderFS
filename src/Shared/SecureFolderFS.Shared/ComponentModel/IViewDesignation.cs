namespace SecureFolderFS.Shared.ComponentModel
{
    /// <summary>
    /// Represents a view which can be notified when it is being navigated to or from.
    /// </summary>
    public interface IViewDesignation : IViewable
    {
        /// <summary>
        /// Notifies the view that it is being navigated to.
        /// </summary>
        void OnAppearing();

        /// <summary>
        /// Notifies the view that it is being navigated from.
        /// </summary>
        void OnDisappearing();
    }
}
