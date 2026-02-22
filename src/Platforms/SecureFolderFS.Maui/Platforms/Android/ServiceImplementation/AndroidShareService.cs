using Android.Content;
using Android.OS;
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
            // Register the file with the ShareContentProvider
            var fileId = ShareContentProvider.RegisterFile(file);

            // Build the content URI for this file
            var authority = $"{Application.Context.PackageName}.shareProvider";
            var contentUri = Uri.Parse($"content://{authority}/{fileId}/{file.Name}");

            // Determine MIME type
            var mimeType = FileTypeHelper.GetMimeType(file) ?? "application/octet-stream";

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
    }
}

