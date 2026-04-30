using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using ByteSizeLib;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OwlCore.Storage;
using SecureFolderFS.Sdk.Attributes;
using SecureFolderFS.Sdk.Extensions;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Shared;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Shared.Extensions;
using SecureFolderFS.Shared.Helpers;
using SecureFolderFS.Storage.StorageProperties;

namespace SecureFolderFS.Sdk.ViewModels.Views.Overlays
{
    [Bindable(true)]
    [Inject<ILocalizationService>, Inject<IClipboardService>, Inject<IMediaService>]
    public sealed partial class PropertiesOverlayViewModel : OverlayViewModel, IWrapper<IStorable>, IAsyncInitialize, IDisposable
    {
        [ObservableProperty] private string? _Id;
        [ObservableProperty] private string? _SizeText;
        [ObservableProperty] private string? _CiphertextId;
        [ObservableProperty] private string? _FileTypeText;
        [ObservableProperty] private string? _DateCreatedText;
        [ObservableProperty] private string? _DateModifiedText;
        [ObservableProperty] private IImage? _Thumbnail;

        /// <inheritdoc/>
        public IStorable Inner { get; }

        public PropertiesOverlayViewModel(IStorable storable)
        {
            ServiceProvider = DI.Default;
            Inner = storable;
            Title = storable.Name;
        }

        /// <inheritdoc/>
        public async Task InitAsync(CancellationToken cancellationToken = default)
        {
            if (Inner is IFile file)
                Thumbnail = await MediaService.TryGenerateThumbnailAsync(file, cancellationToken: cancellationToken);

            var typeClassification = FileTypeHelper.GetClassification(Inner);
            FileTypeText = Inner is IFolder ? "inode/directory" : typeClassification.MimeType;

            if (Inner is ISizeOf sizeOf)
            {
                var size = await sizeOf.SizeOf.GetValueAsync(cancellationToken);
                if (size is not null)
                    SizeText = ByteSize.FromBytes(size.Value).ToString();
            }

            if (Inner is ILastModifiedAt lastModifiedAt)
            {
                var lastModifiedAtDate = await lastModifiedAt.LastModifiedAt.GetValueAsync(cancellationToken);
                if (lastModifiedAtDate is not null)
                    DateModifiedText = LocalizationService.LocalizeDate(lastModifiedAtDate.Value);
            }

            if (Inner is ICreatedAt createdAt)
            {
                var createdAtDate = await createdAt.CreatedAt.GetValueAsync(cancellationToken);
                if (createdAtDate is not null)
                    DateCreatedText = LocalizationService.LocalizeDate(createdAtDate.Value);
            }

            Id = Inner.Id;
            CiphertextId = Inner switch
            {
                IFolder => Inner.AsWrapper<IFolder>().GetWrapperAt("CryptoFolder").Inner.Id,
                IFile => Inner.AsWrapper<IFile>().GetWrapperAt("CryptoFile").Inner.Id,
                _ => CiphertextId
            };
        }

        [RelayCommand]
        private async Task CopyPropertyAsync(string? propertyName, CancellationToken cancellationToken)
        {
            var propertyValue = propertyName switch
            {
                "ItemType" => FileTypeText,
                "DateModified" => DateModifiedText,
                "Size" => SizeText,
                "Path" => Id,
                "EncryptedPath" => CiphertextId,
                _ => null
            };

            if (propertyValue is not null && await ClipboardService.IsSupportedAsync())
                await ClipboardService.SetTextAsync(propertyValue, cancellationToken);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            Thumbnail?.Dispose();
            Thumbnail = null;
        }
    }
}
