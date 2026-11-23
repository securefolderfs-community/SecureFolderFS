using Microsoft.Extensions.DependencyInjection;
using OwlCore.Storage;
using SecureFolderFS.Sdk.Api.Services;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Shared.Extensions;
using SecureFolderFS.Shared.Models;
using SecureFolderFS.UI.ServiceImplementation;
using SecureFolderFS.UI.ServiceImplementation.Settings;
using SecureFolderFS.Uno.ServiceImplementation;
using AddService = Microsoft.Extensions.DependencyInjection.ServiceCollectionServiceExtensions;

namespace SecureFolderFS.Uno.Extensions
{
    internal static class UnoIocExtensions
    {
        public static IServiceCollection WithUnoServices(this IServiceCollection serviceCollection, IModifiableFolder settingsFolder)
        {
            return serviceCollection
                    .WithLocalIntegrationApi(settingsFolder)
                    .Foundation<ISettingsService, SettingsService>(AddService.AddSingleton, _ => new(new AppSettings(settingsFolder), new UserSettings(settingsFolder)))
                    .Foundation<IMediaService, UnoMediaService>(AddService.AddSingleton)
                    .Foundation<IOverlayService, UnoDialogService>(AddService.AddSingleton)
                    .Foundation<IStorageService, UnoStorageService>(AddService.AddSingleton)
                    .Foundation<IClipboardService, UnoClipboardService>(AddService.AddSingleton)
                    .Foundation<IThreadingService, UnoThreadingService>(AddService.AddSingleton)
                    .Foundation<IFileExplorerService, UnoFileExplorerService>(AddService.AddSingleton)
                    .Foundation<INavigationService, UnoNavigationService>(AddService.AddTransient)
                ;
        }
        
        private static IServiceCollection WithLocalIntegrationApi(this IServiceCollection serviceCollection, IModifiableFolder settingsFolder)
        {
            return serviceCollection
                    .Foundation<IPairingStore, PairingStore>(AddService.AddSingleton, _ => new PairingStore(UI.Constants.FileNames.API_CLIENTS_FILENAME, settingsFolder, StreamSerializer.Instance))
                    .Foundation<IVaultApiBridge, UnoVaultApiBridge>(AddService.AddSingleton, sp => new UnoVaultApiBridge(sp.GetRequiredService<IPairingStore>()))
                    .Foundation<IApiConsentService, UnoApiConsentService>(AddService.AddSingleton)
                    .Foundation<IPeerEvidenceProvider, PeerEvidenceProvider>(AddService.AddSingleton)
                    .Foundation<IApiHost, ApiHost>(AddService.AddSingleton, sp => new ApiHost(
                        sp.GetRequiredService<IVaultApiBridge>(),
                        sp.GetRequiredService<IPairingStore>(),
                        sp.GetRequiredService<IApiConsentService>(),
                        sp.GetRequiredService<IPeerEvidenceProvider>(),
                        sp.GetRequiredService<IApplicationService>().AppVersion.ToString()))
                    .Foundation<ILocalIntegrationsService, UnoLocalIntegrationsService>(AddService.AddSingleton)
                ;
        }
    }
}
