using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using OwlCore.Storage;
using SecureFolderFS.Core.FileSystem;
using SecureFolderFS.Core.FileSystem.Helpers.Paths.Abstract;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.ViewModels.Views.Overlays;
using SecureFolderFS.Sdk.ViewModels.Views.Vault;
using SecureFolderFS.Shared;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Shared.Extensions;
using SecureFolderFS.Storage.SystemStorageEx;
using SecureFolderFS.UI.Utils;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace SecureFolderFS.Uno.Views.Vault
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    [INotifyPropertyChanged]
    public sealed partial class VaultOverviewPage : Page, IEmbeddedControlContent
    {
        public VaultOverviewViewModel? ViewModel
        {
            get => DataContext.TryCast<VaultOverviewViewModel>();
            set { DataContext = value; OnPropertyChanged(); }
        }

        /// <inheritdoc/>
        public object? EmbeddedContent { get => field ??= CreateEmbeddedContent(); }

        public VaultOverviewPage()
        {
            InitializeComponent();
        }

        /// <inheritdoc/>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter is VaultOverviewViewModel viewModel)
                ViewModel = viewModel;

            base.OnNavigatedTo(e);
        }

        private DependencyObject? CreateEmbeddedContent()
        {
            if (Resources.TryGetValue("WidgetReorderButtonTemplate", out var resource) && resource is DataTemplate template)
            {
                var content = template.LoadContent();
                if (content is FrameworkElement element)
                    element.DataContext = ViewModel;

                return content;
            }

            return null;
        }

        private void VaultOptions_DragOver(object sender, DragEventArgs e)
        {
            if (e.DataView.Contains(StandardDataFormats.StorageItems))
                e.AcceptedOperation = DataPackageOperation.Link;

            e.Handled = true;
        }

        private async void VaultOptions_Drop(object sender, DragEventArgs e)
        {
            if (ViewModel?.VaultViewModel.VaultModel.VaultFolder is null)
                return;
            
            var storageRoot = ViewModel.UnlockedVaultViewModel.StorageRoot;
            if (storageRoot is not IWrapper<FileSystemSpecifics> { Inner: var specifics })
                return;
            
            var deferral = e.GetDeferral();
            try
            {
                // We only want to get the first item
                var droppedItems = await e.DataView.GetStorageItemsAsync().AsTask();
                var item = droppedItems.FirstOrDefault();
                if (item is null)
                    return;

                var itemPath = item.Path.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
                var vaultFolderId = ViewModel.VaultViewModel.VaultModel.VaultFolder.Id.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
                var virtualizedRootId = storageRoot.VirtualizedRoot.Id.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

                IStorable? ciphertextItem = null;
                if (itemPath.Contains(vaultFolderId))
                {
                    // Ciphertext path
                    ciphertextItem = item switch
                    {
                        IStorageFile => new SystemFileEx(itemPath),
                        IStorageFolder => new SystemFolderEx(itemPath),
                        _ => null
                    };
                }
                else if (itemPath.Contains(virtualizedRootId))
                {
                    var pathRoot = Path.GetPathRoot(itemPath) ?? string.Empty;
                    var relativePath = $"{Path.DirectorySeparatorChar}{itemPath.Replace(pathRoot, string.Empty)}";
                    ciphertextItem = await storageRoot.PlaintextRoot.GetItemByRelativePathAsync(relativePath) switch
                    {
                        IWrapper<IFile> fileWrapper => fileWrapper.Inner,
                        IWrapper<IFolder> folderWrapper => folderWrapper.Inner,
                        _ => null
                    };
                }
                
                if (ciphertextItem is not IStorableChild ciphertextChild)
                    return;

                var plaintextPath = await AbstractPathHelpers.GetPlaintextPathAsync(ciphertextChild, specifics, CancellationToken.None);
                if (string.IsNullOrEmpty(plaintextPath))
                    return;

                var plaintextItem = await storageRoot.PlaintextRoot.GetItemByRelativePathAsync(plaintextPath);
                _ = DisplayInformation();
                
                async Task DisplayInformation()
                {
                    var viewModel = new VaultItemInfoOverlayViewModel(ciphertextItem, plaintextItem);
                    await viewModel.InitAsync();
                
                    var overlayService = DI.Service<IOverlayService>();
                    _ = overlayService.ShowAsync(viewModel);
                }
            }
            catch (Exception)
            {
            }
            finally
            {
                e.Handled = true;
                deferral.Complete();
            }
        }
    }
}
