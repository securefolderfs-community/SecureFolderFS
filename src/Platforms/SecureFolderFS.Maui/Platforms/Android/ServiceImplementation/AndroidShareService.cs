using Android.Content;
using OwlCore.Storage;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Shared.Helpers;
using Application = Android.App.Application;
using Uri = Android.Net.Uri;

namespace SecureFolderFS.Maui.Platforms.Android.ServiceImplementation
{
    /// <inheritdoc cref="IShareService"/>
    internal sealed class AndroidShareService : IShareService
    {
        /// <inheritdoc/>
        public async Task ShareTextAsync(string text, string title)
        {
            var intent = new Intent(Intent.ActionSend);
            intent.SetType("text/plain");
            intent.PutExtra(Intent.ExtraText, text);
            intent.PutExtra(Intent.ExtraSubject, title);

            var chooserIntent = Intent.CreateChooser(intent, title);
            chooserIntent?.AddFlags(ActivityFlags.NewTask);

            Application.Context.StartActivity(chooserIntent);
            await Task.CompletedTask;
        }

        /// <inheritdoc/>
        public async Task ShareFileAsync(IFile file)
        {
            // Build the content URI for this file
            var contentUri = ShareContentProvider.RegisterFileAndBuildUri(Application.Context, file);

            // Determine MIME type
            var mimeType = FileTypeHelper.GetMimeType(file.Name);

            // Create share intent
            var intent = new Intent(Intent.ActionSend);
            intent.SetType(mimeType);
            intent.PutExtra(Intent.ExtraStream, contentUri);
            intent.AddFlags(ActivityFlags.GrantReadUriPermission);

            var chooserIntent = Intent.CreateChooser(intent, file.Name);
            chooserIntent?.AddFlags(ActivityFlags.NewTask);

            Application.Context.StartActivity(chooserIntent);
            await Task.CompletedTask;
        }

        /// <inheritdoc/>
        public Task OpenFileWithAsync(IFile file)
        {
            // Build the content URI for this file
            var contentUri = ShareContentProvider.RegisterFileAndBuildUri(Application.Context, file);

            // Determine MIME type
            var mimeType = FileTypeHelper.GetMimeType(file.Name);

            // Create view intent
            var intent = new Intent(Intent.ActionView);
            intent.SetDataAndType(contentUri, mimeType);
            intent.AddFlags(ActivityFlags.GrantReadUriPermission);

            var chooserIntent = Intent.CreateChooser(intent, file.Name);
            chooserIntent?.AddFlags(ActivityFlags.NewTask);

            Application.Context.StartActivity(chooserIntent);
            return Task.CompletedTask;
        }
    }
}
