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
using SecureFolderFS.Shared.Helpers;
using SecureFolderFS.Storage.StorageProperties;

namespace SecureFolderFS.Sdk.ViewModels.Views.Overlays
{
    [Bindable(true)]
    [Inject<ILocalizationService>, Inject<IClipboardService>, Inject<IMediaService>]
    public sealed partial class PropertiesOverlayViewModel : OverlayViewModel, IWrapper<IStorable>, IAsyncInitialize, IDisposable
    {
        private readonly IBasicProperties _properties;

        [ObservableProperty] private string? _SizeText;
        [ObservableProperty] private string? _FileTypeText;
        [ObservableProperty] private string? _DateCreatedText;
        [ObservableProperty] private string? _DateModifiedText;
        [ObservableProperty] private IImage? _Thumbnail;

        /// <inheritdoc/>
        public IStorable Inner { get; }

        public PropertiesOverlayViewModel(IStorable storable, IBasicProperties properties)
        {
            ServiceProvider = DI.Default;
            Inner = storable;
            _properties = properties;
            Title = storable.Name;
        }

        /// <inheritdoc/>
        public async Task InitAsync(CancellationToken cancellationToken = default)
        {
            if (Inner is IFile file)
                Thumbnail = await MediaService.TryGenerateThumbnailAsync(file, cancellationToken: cancellationToken);

            var typeClassification = FileTypeHelper.GetClassification(Inner);
            FileTypeText = Inner is IFolder ? "inode/directory" : typeClassification.MimeType;

            if (_properties is ISizeProperties sizeProperties)
            {
                var sizeProperty = await sizeProperties.GetSizeAsync(cancellationToken);
                if (sizeProperty is not null)
                    SizeText = ByteSize.FromBytes(sizeProperty.Value).ToString();
            }

            if (_properties is IDateProperties dateProperties)
            {
                var dateModifiedProperty = await dateProperties.GetDateModifiedAsync(cancellationToken);
                DateModifiedText = LocalizationService.LocalizeDate(dateModifiedProperty.Value);

                var dateCreatedProperty = await dateProperties.GetDateCreatedAsync(cancellationToken);
                DateCreatedText = LocalizationService.LocalizeDate(dateCreatedProperty.Value);
            }
        }

        [RelayCommand]
        private async Task CopyPropertyAsync(string? propertyName, CancellationToken cancellationToken)
        {
            var propertyValue = propertyName switch
            {
                "ItemType" => FileTypeText,
                "DateModified" => DateModifiedText,
                "Size" => SizeText,
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
