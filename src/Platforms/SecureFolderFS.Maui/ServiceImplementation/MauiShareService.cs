using SecureFolderFS.Sdk.Services;

namespace SecureFolderFS.Maui.ServiceImplementation
{
    /// <inheritdoc cref="IShareService"/>
    internal sealed class MauiShareService : IShareService
    {
        /// <inheritdoc/>
        public async Task ShareTextAsync(string text, string title)
        {
            await Share.Default.RequestAsync(new ShareTextRequest()
            {
                Text = text,
                Title = title
            });
        }
    }
}
