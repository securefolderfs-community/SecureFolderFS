using System;
using System.ComponentModel;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OwlCore.Storage;
using SecureFolderFS.Sdk.AppModels;
using SecureFolderFS.Sdk.Attributes;
using SecureFolderFS.Sdk.Extensions;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Shared;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Shared.Enums;
using SecureFolderFS.Shared.Extensions;
using SecureFolderFS.Shared.Helpers;
using SecureFolderFS.Shared.Models;

namespace SecureFolderFS.Sdk.ViewModels.Controls.Storage.Browser
{
    [Inject<IMediaService>, Inject<ISettingsService>]
    [Bindable(true)]
    public sealed partial class SearchBrowserItemViewModel : StorageItemViewModel, IAsyncInitialize
    {
        private readonly ThumbnailCacheModel _thumbnailCache;
        private readonly SynchronizationContext? _uiContext;
        private readonly Func<SearchBrowserItemViewModel, CancellationToken, Task> _openSearchResultAsync;

        [ObservableProperty] private string? _Id;

        /// <inheritdoc/>
        public override IStorable Inner { get; }

        public TypeClassification? Classification { get; }

        public SearchBrowserItemViewModel(
            IStorable storable,
            ThumbnailCacheModel thumbnailCache,
            Func<SearchBrowserItemViewModel, CancellationToken, Task> openSearchResultAsync)
        {
            ServiceProvider = DI.Default;
            _uiContext = SynchronizationContext.Current;
            Inner = storable;
            Id = storable.Id;
            _thumbnailCache = thumbnailCache;
            _openSearchResultAsync = openSearchResultAsync;

            if (storable is IFile file)
            {
                Title = !SettingsService.UserSettings.AreFileExtensionsEnabled
                    ? Path.GetFileNameWithoutExtension(file.Name)
                    : file.Name;
                Classification = FileTypeHelper.GetClassification(file);
            }
            else
            {
                Title = storable.Name;
            }
        }

        /// <inheritdoc/>
        public async Task InitAsync(CancellationToken cancellationToken = default)
        {
            Thumbnail?.Dispose();

            if (!CanLoadThumbnail() || Inner is not IFile file || Classification is not { TypeHint: var typeHint })
                return;

            var cacheKey = await ThumbnailCacheModel.GetCacheKeyAsync(file, cancellationToken).ConfigureAwait(false);
            var cachedStream = await _thumbnailCache.TryGetCachedThumbnailAsync(cacheKey, cancellationToken).ConfigureAwait(false);
            if (cachedStream is not null)
            {
                await _uiContext.PostOrExecuteAsync(() =>
                {
                    Thumbnail = new StreamImageModel(cachedStream);
                    return Task.CompletedTask;
                });
                return;
            }

            cancellationToken.ThrowIfCancellationRequested();
            var generatedThumbnail = await MediaService.TryGenerateThumbnailAsync(file, typeHint, cancellationToken).ConfigureAwait(false);
            if (generatedThumbnail is null)
                return;

            await _uiContext.PostOrExecuteAsync(() =>
            {
                Thumbnail = generatedThumbnail;
                return Task.CompletedTask;
            });

            _ = _thumbnailCache.CacheThumbnailAsync(cacheKey, generatedThumbnail, cancellationToken);
        }

        public bool CanLoadThumbnail()
        {
            return SettingsService.UserSettings.AreThumbnailsEnabled
                && Inner is IFile
                && Classification is { TypeHint: TypeHint.Image or TypeHint.Media };
        }

        [RelayCommand]
        private Task OpenAsync(CancellationToken cancellationToken)
        {
            return _openSearchResultAsync(this, cancellationToken);
        }
    }
}
