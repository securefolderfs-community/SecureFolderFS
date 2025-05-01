using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using ByteSizeLib;
using CommunityToolkit.Mvvm.ComponentModel;
using OwlCore.Storage;
using SecureFolderFS.Sdk.Attributes;
using SecureFolderFS.Sdk.Extensions;
using SecureFolderFS.Sdk.Helpers;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Shared;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Storage.StorageProperties;

namespace SecureFolderFS.Sdk.ViewModels.Views.Overlays
{
    [Bindable(true)]
    [Inject<ILocalizationService>]
    public sealed partial class PropertiesOverlayViewModel : OverlayViewModel, IAsyncInitialize
    {
        private readonly IStorable _storable;
        private readonly IBasicProperties _properties;

        [ObservableProperty] private string? _SizeText;
        [ObservableProperty] private string? _FileTypeText;
        [ObservableProperty] private string? _DateModifiedText;

        public PropertiesOverlayViewModel(IStorable storable, IBasicProperties properties)
        {
            ServiceProvider = DI.Default;
            _storable = storable;
            _properties = properties;
            Title = _storable.Name;
        }

        /// <inheritdoc/>
        public async Task InitAsync(CancellationToken cancellationToken = default)
        {
            var typeClassification = FileTypeHelper.GetClassification(_storable);
            FileTypeText = _storable is IFolder ? "inode/directory" : typeClassification.MimeType;

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
            }
        }
    }
}
